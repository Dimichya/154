using System.Collections.Concurrent;
using System;
using System.IO;
using Shared.Models.Module;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Shared.Models.Base;
using Shared;

namespace CustomUsersDB
{
    public static class FileDB
    {
        static DateTime lastlastWriteTime;

        public static void Load(InitspaceModel conf, string path)
        {
            if (!File.Exists($"{conf.path}/{path}"))
                return;

            var lastWriteTime = File.GetLastWriteTime($"{conf.path}/{path}");
            if (lastlastWriteTime == lastWriteTime)
                return;

            lastlastWriteTime = lastWriteTime;
            var users = new ConcurrentBag<AccsUser>();

            if (path.EndsWith(".json"))
            {
                AppInit.conf.accsdb.users = JsonConvert.DeserializeObject<ConcurrentBag<AccsUser>>(File.ReadAllText($"{conf.path}/{path}"));
            }
            else if (path.EndsWith(".txt"))
            {
                foreach (string _lne in File.ReadAllLines($"{conf.path}/{path}"))
                {
                    string line = _lne.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("/"))
                        continue;

                    try
                    {
                        if (line.StartsWith("{"))
                        {
                            if (line.Contains("\"id\"") && line.Contains("\"expires\""))
                            {
                                users.Add(JsonConvert.DeserializeObject<AccsUser>(line));
                            }
                            else
                            {
                                var node = JsonNode.Parse(line);
                                if (node["access"].GetValue<bool>())
                                {
                                    users.Add(new AccsUser()
                                    {
                                        id = node["token"].GetValue<string>(),
                                        expires = DateTime.Today.AddYears(10),
                                    });
                                }
                            }
                        }
                        else
                        {
                            users.Add(new AccsUser()
                            {
                                id = line.Split('/')[0].Trim(),
                                expires = DateTime.Today.AddYears(10),
                                //group = 0,
                                //ban = false,
                                //ban_msg = null,
                                //comment = null,
                                //ids = new List<string> { "uid2", "uid3" },
                                //@params = new Dictionary<string, object> 
                                //{
                                //    ["adult"] = true,
                                //    ["dataserver"] = DateTime.Now,
                                //    ["etc"] = 1,
                                //    ["etc2"] = new List<string> { "uid2", "uid3" },
                                //}
                            });
                        }
                    }
                    catch { }
                }
            }
            else { return; }

            AppInit.conf.accsdb.users = users;
            File.WriteAllText("current.conf", JsonConvert.SerializeObject(AppInit.conf, Formatting.Indented));
        }
    }
}
