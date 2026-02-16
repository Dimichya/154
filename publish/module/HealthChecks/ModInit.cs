using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shared.Models.Module;
using Telegram.Bot;

namespace HealthChecks
{
    public class ModInit
    {
        public static void loaded(InitspaceModel conf)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (true)
                {
                    try
                    {
                        string manifestPath = Path.Combine(conf.path, "manifest.json");
                        if (!File.Exists(manifestPath))
                        {
                            Console.WriteLine($"HealthChecks: manifest not found: {manifestPath}");
                            return;
                        }

                        var manifestJson = File.ReadAllText(manifestPath);
                        JObject root = null;
                        try { root = JObject.Parse(manifestJson); } catch { Console.WriteLine("HealthChecks: invalid manifest.json"); return; }

                        int intervalMinutes = 20;
                        if (root.TryGetValue("interval", out var intervalToken) && intervalToken.Type == JTokenType.Integer)
                            intervalMinutes = intervalToken.Value<int>();

                        await Task.Delay(TimeSpan.FromMinutes(intervalMinutes)).ConfigureAwait(false);

                        if (!(root.TryGetValue("checks", out var checksToken) && checksToken is JArray checksElem))
                        {
                            Console.WriteLine("HealthChecks: no checks defined in manifest");
                            continue;
                        }

                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (HealthChecks)");

                        foreach (var ch in checksElem)
                        {
                            if (!(ch is JObject chObj))
                                continue;

                            string uri = chObj.Value<string>("uri");

                            var ignoreList = new string[0];
                            if (chObj.TryGetValue("ignore", out var ignoreToken) && ignoreToken is JArray ignoreArr)
                                ignoreList = ignoreArr.Where(x => x.Type == JTokenType.String).Select(x => x.Value<string>()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                            if (string.IsNullOrEmpty(uri))
                                continue;

                            try
                            {
                                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
                                var resp = await httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
                                if (!resp.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"HealthChecks: GET {uri} -> {(int)resp.StatusCode} {resp.ReasonPhrase}");
                                    continue;
                                }

                                string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                                JToken parsedRoot;
                                try
                                {
                                    parsedRoot = JToken.Parse(body);
                                }
                                catch
                                {
                                    Console.WriteLine($"HealthChecks: invalid json from {uri}");
                                    continue;
                                }

                                if (parsedRoot.Type != JTokenType.Array)
                                {
                                    Console.WriteLine($"HealthChecks: json is not an array from {uri}");
                                    continue;
                                }

                                var disabledByBalancer = new System.Collections.Generic.List<string>();

                                foreach (var item in parsedRoot as JArray)
                                {
                                    if (item.Type != JTokenType.Object)
                                        continue;

                                    bool show = true;
                                    var showProp = item["show"];
                                    if (showProp != null && showProp.Type == JTokenType.Boolean && showProp.Value<bool>() == false)
                                        show = false;

                                    if (show)
                                        continue;

                                    string balancer = item.Value<string>("balanser");
                                    if (string.IsNullOrEmpty(balancer))
                                        continue;

                                    if (ignoreList != null && ignoreList.Length > 0)
                                    {
                                        bool skip = false;
                                        foreach (var ig in ignoreList)
                                        {
                                            if (string.IsNullOrEmpty(ig))
                                                continue;

                                            if (balancer.IndexOf(ig, StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                skip = true; break;
                                            }
                                        }

                                        if (skip) continue;
                                    }

                                    if (!disabledByBalancer.Contains(balancer))
                                        disabledByBalancer.Add(balancer);
                                }

                                if (disabledByBalancer.Count > 0)
                                {
                                    try
                                    {
                                        bool fileLog = true;
                                        string msg = $"{uri} found {disabledByBalancer.Count} disabled balancers\n\n  - " + string.Join("\n  - ", disabledByBalancer);

                                        if (root.TryGetValue("telegram", out var tg) && tg is JObject tgObj)
                                        {
                                            string tgToken = tgObj.Value<string>("token");
                                            string tgChat = tgObj.Value<string>("chat_id");

                                            if (!string.IsNullOrEmpty(tgToken) && !string.IsNullOrEmpty(tgChat))
                                            {
                                                fileLog = false;
                                                await TrySendTelegram(tgToken, tgChat, msg).ConfigureAwait(false);
                                            }
                                        }

                                        if (fileLog)
                                            File.AppendAllText(Path.Combine(conf.path, "log.txt"), msg + "\n\n\n==============================\n\n");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("HealthChecks: telegram send exception: " + ex.Message);
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine($"HealthChecks: request timeout (40s) for {uri}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"HealthChecks: exception for {uri} -> {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("HealthChecks main loop exception: " + ex.Message);
                        await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                    }
                }
            });
        }


        static async ValueTask TrySendTelegram(string token, string chatId, string text)
        {
            using (var cts = new CancellationTokenSource())
            {
                var bot = new TelegramBotClient(token, cancellationToken: cts.Token);

                await bot.SendMessage(chatId, text, cancellationToken: cts.Token).ConfigureAwait(false);
            }
        }
    }
}
