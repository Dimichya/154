using BwaRC;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Engine;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using IO = System.IO;

namespace Lampac.Controllers.BwaRC
{
    public class BindController : BaseOnlineController
    {
        #region saveBind
        void saveBind(string user_id, JObject ob)
        {
            if (string.IsNullOrEmpty(user_id))
                return;

            string init_file = $"{ModInit.folder_mod}/raw/{CrypTo.md5(user_id.ToLower().Trim())}";
            IO.File.WriteAllText(init_file, JsonConvert.SerializeObject(ob, Formatting.Indented));
        }
        #endregion

        #region renderHtml
        ContentResult renderHtml(string title, string body)
        {
            return Content(@"<!DOCTYPE html>
<html>
<head>
    <title>" + title + @"</title>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta charset='utf-8'>
    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body {
            background-color: #fafafa;
            color: #ffffff;
            font-family: sans-serif;
            line-height: 1.6;
            padding: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }

        .container {
            background-color: #fdfdfd;
            padding: 30px;
            border-radius: 8px;
            max-width: 600px;
            width: 100%;
            box-shadow: 0 18px 24px rgb(175 175 175 / 30%);
            color: #000;
        }

        .steps {
            margin: 20px 0;
        }

        .step {
            margin-bottom: 20px;
            font-size: 18px;
        }

        .title {
            margin-bottom: 20px;
            font-size: 18px;
        }

        .code {
            background-color: #ffffff;
            padding: 15px;
            border-radius: 4px;
            font-size: 24px;
            font-weight: bold;
            text-align: center;
            margin: 10px 0;
            border: 1px solid #eaeaea;
            color: #4caf50;
        }

        a {
            color: #64B5F6;
            text-decoration: none;
            transition: color 0.3s;
        }

        a:hover {
            color: #90CAF9;
        }

        .button {
            display: inline-block;
            background-color: #8bc34a;
            color: white;
            padding: 12px 24px;
            border-radius: 4px;
            text-decoration: none;
            margin-top: 20px;
            transition: background-color 0.3s;
            border: none;
            font-size: 16px;
            cursor: pointer;
            width: 100%;
            text-align: center;
        }

        .button:hover {
            background-color: #45a049;
            color: #fff
        }

        .form-group {
            margin-bottom: 15px;
        }

        input[type='text'] {
            background-color: #ffffff;
            border: 1px solid #dbdbdb;
            color: #4e4b4b;
            padding: 12px;
            border-radius: 4px;
            width: 100%;
            margin-bottom: 10px;
            font-size: 16px;
        }

        input[type='text']:focus {
            border-color: #4CAF50;
            outline: none;
        }

        @media (max-width: 480px) {
            body {
                padding: 15px;
            }

            .container {
                padding: 20px;
            }

            .step {
                font-size: 16px;
            }

            .code {
                font-size: 20px;
            }
        }
    </style>
</head>
<body>
    <div class='container'>
        " + body + @"
    </div>
</body>
</html>", "text/html; charset=utf-8");
        }
        #endregion

        #region loadconf
        JObject loadconf(string user_id = null)
        {
            if (string.IsNullOrEmpty(user_id))
            {
                user_id = HttpContext.Request.Query["token"].ToString();
                if (string.IsNullOrEmpty(user_id))
                {
                    user_id = HttpContext.Request.Query["account_email"].ToString();
                    if (string.IsNullOrEmpty(user_id))
                        user_id = HttpContext.Request.Query["uid"].ToString();
                }
            }

            if (string.IsNullOrEmpty(user_id))
                return new JObject();

            string init_file = $"{ModInit.folder_mod}/raw/{CrypTo.md5(user_id.ToLower().Trim())}";
            if (!IO.File.Exists(init_file))
                return new JObject();

            return JsonConvert.DeserializeObject<JObject>(IO.File.ReadAllText(init_file));
        }
        #endregion


        #region Filmix
        [HttpGet]
        [Route("/bind/filmix")]
        async public Task<ActionResult> Filmix(string user_id, string filmix_token)
        {
            if (string.IsNullOrEmpty(filmix_token))
            {
                var token_request = await Http.Get<JObject>("http://filmixapp.vip/api/v2/token_request?user_dev_apk=2.2.0&user_dev_id=&user_dev_name=Xiaomi&user_dev_os=11&user_dev_vendor=Xiaomi&user_dev_token=", timeoutSeconds: 10, useDefaultHeaders: false);

                if (token_request == null)
                    return Content("filmixapp.vip недоступен, повторите попытку позже", "text/html; charset=utf-8");

                string body = $@"<div class='steps'>
                    <div class='step'>
                        1. Откройте <a href='https://filmix.my/consoles' target='_blank'>https://filmix.my/consoles</a>
                    </div>
                    <div class='step'>
                        2. Добавьте идентификатор устройства
                        <div class='code'>{token_request.Value<string>("user_code")}</div>
                    </div>
                </div>
                <a href='/bind/filmix?user_id={HttpUtility.UrlEncode(user_id)}&filmix_token={token_request.Value<string>("code")}' class='button'>
                    завершить привязку устройства
                </a>";

                return renderHtml("Привязка Filmix", body);
            }
            else
            {
                if (string.IsNullOrEmpty(user_id))
                    return Content("ошибка, отсутствует параметр user_id", "text/html; charset=utf-8");

                bool pro = false;
                var root = await Http.Get<JObject>("http://filmixapp.vip/api/v2/user_profile?app_lang=ru_RU&user_dev_apk=2.2.0&user_dev_id=&user_dev_name=Xiaomi&user_dev_os=11&user_dev_vendor=Xiaomi&user_dev_token=" + filmix_token, timeoutSeconds: 10, useDefaultHeaders: false);
                if (root != null)
                {
                    if (!root.ContainsKey("user_data"))
                        return Content($"Указанный токен {filmix_token} не найден", "text/html; charset=utf-8");

                    var user_data = root["user_data"];
                    if (user_data != null)
                    {
                        pro = user_data.Value<bool>("is_pro");
                        if (pro == false)
                            pro = user_data.Value<bool>("is_pro_plus");
                    }
                }

                var bwaconf = loadconf(user_id);
                bwaconf["Filmix"] = new JObject()
                {
                    ["enable"] = true,
                    ["token"] = filmix_token,
                    ["pro"] = pro,
                };

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion

        #region Vokino
        [HttpGet]
        [Route("/bind/vokino")]
        async public Task<ActionResult> Vokino(string user_id, string login, string pass)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                string body = $@"<div class='title'>Введите данные аккаунта <a href='http://vokino.tv' target='_blank'>vokino.tv</a></div>
                    <form method='get' action='/bind/vokino'>
                        <input type='hidden' name='user_id' value='{user_id}'>
                        <div class='form-group'>
                            <input type='text' name='login' placeholder='Email' required>
                        </div>
                        <div class='form-group'>
                            <input type='text' name='pass' placeholder='Пароль' required>
                        </div>
                        <button type='submit' class='button'>Добавить устройство</button>
                    </form>";

                return renderHtml("Привязка VoKino", body);
            }
            else
            {
                if (string.IsNullOrEmpty(user_id))
                    return Content("ошибка, отсутствует параметр user_id", "text/html; charset=utf-8");

                string deviceid = new string(DateTime.Now.ToBinary().ToString().Reverse().ToArray()).Substring(0, 8);
                var token_request = await Http.Get<JObject>($"http://api.vokino.tv/v2/auth?email={HttpUtility.UrlEncode(login)}&passwd={HttpUtility.UrlEncode(pass)}&deviceid={deviceid}", timeoutSeconds: 10, headers: HeadersModel.Init(("user-agent", "lampac")));

                if (token_request == null)
                    return Content("api.vokino.tv недоступен, повторите попытку позже", "text/html; charset=utf-8");

                string authToken = token_request.Value<string>("authToken");
                if (string.IsNullOrEmpty(authToken))
                    return Content(token_request.Value<string>("error") ?? "Не удалось получить токен", "text/html; charset=utf-8");

                var bwaconf = loadconf(user_id);
                bwaconf["VoKino"] = new JObject()
                {
                    ["enable"] = true,
                    ["token"] = authToken,
                    ["online"] = new JObject()
                    {
                        ["vokino"] = AppInit.conf.VoKino.online.vokino,
                        ["filmix"] = AppInit.conf.VoKino.online.filmix,
                        ["alloha"] = AppInit.conf.VoKino.online.alloha,
                        ["monframe"] = AppInit.conf.VoKino.online.monframe,
                        ["remux"] = AppInit.conf.VoKino.online.remux,
                        ["ashdi"] = AppInit.conf.VoKino.online.ashdi,
                        ["hdvb"] = AppInit.conf.VoKino.online.hdvb,
                        ["vibix"] = AppInit.conf.VoKino.online.vibix
                    }
                };

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion

        #region Kinopub
        [HttpGet]
        [Route("/bind/kinopub")]
        async public Task<ActionResult> Kinopub(string user_id, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                var token_request = await Http.Post<JObject>("https://api.srvkp.com/oauth2/device?grant_type=device_code&client_id=xbmc&client_secret=cgg3gtifu46urtfp2zp1nqtba0k2ezxh", "", timeoutSeconds: 10);
                if (token_request == null || string.IsNullOrWhiteSpace(token_request.Value<string>("user_code")))
                    return Content("api.srvkp.com недоступен, повторите попытку позже", "text/html; charset=utf-8");

                string body = $@"<div class='steps'>
                    <div class='step'>
                        1. Откройте <a href='https://kino.pub/device' target='_blank'>https://kino.pub/device</a>
                    </div>
                    <div class='step'>
                        2. Введите код устройства
                        <div class='code'>{token_request.Value<string>("user_code")}</div>
                    </div>
                </div>
                <a href='/bind/kinopub?user_id={HttpUtility.UrlEncode(user_id)}&code={token_request.Value<string>("code")}' class='button'>
                    завершить привязку устройства
                </a>";

                return renderHtml("Привязка KinoPub", body);
            }
            else
            {
                if (string.IsNullOrEmpty(user_id))
                    return Content("ошибка, отсутствует параметр user_id", "text/html; charset=utf-8");

                var device_token = await Http.Post<JObject>($"https://api.srvkp.com/oauth2/device?grant_type=device_token&client_id=xbmc&client_secret=cgg3gtifu46urtfp2zp1nqtba0k2ezxh&code={code}", "");
               
                if (device_token == null)
                    return Content("api.srvkp.com недоступен, повторите попытку позже", "text/html; charset=utf-8");

                if (string.IsNullOrWhiteSpace(device_token.Value<string>("access_token")))
                    return Content($"Указанный токен {device_token.Value<string>("access_token")} не найден", "text/html; charset=utf-8");

                var bwaconf = loadconf(user_id);
                bwaconf["KinoPub"] = new JObject()
                {
                    ["enable"] = true,
                    ["token"] = device_token.Value<string>("access_token")
                };

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion

        #region Rezka
        [HttpGet]
        [Route("/bind/rezka")]
        async public Task<ActionResult> Rezka(string user_id, string login, string pass)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                string body = $@"<div class='title'>Введите данные аккаунта <a href='{AppInit.conf.Rezka.host}' target='_blank'>hdrezka.me</a></div>
                    <form method='get' action='/bind/rezka'>
                        <input type='hidden' name='user_id' value='{user_id}'>
                        <div class='form-group'>
                            <input type='text' name='login' placeholder='Email' required>
                        </div>
                        <div class='form-group'>
                            <input type='text' name='pass' placeholder='Пароль' required>
                        </div>
                        <button type='submit' class='button'>Привязать устройство</button>
                    </form>";

                return renderHtml("Привязка HDRezka", body);
            }
            else
            {
                string cookie = await getCookieRezka(login, pass);
                if (string.IsNullOrEmpty(cookie))
                    return Content("Ошибка авторизации, повторите попытку позже", "text/html; charset=utf-8");

                bool premium = false;
                string rezka_main = await Http.Get(AppInit.conf.Rezka.host, cookie: cookie, timeoutSeconds: 10);
                if (rezka_main != null && (rezka_main.Contains("b-premium_user__body") || rezka_main.Contains("b-hd_prem")))
                    premium = true;

                var bwaconf = loadconf(user_id);

                if (premium)
                {
                    bwaconf["RezkaPrem"] = new JObject()
                    {
                        ["enable"] = true,
                        ["cookie"] = cookie
                    };

                    if (bwaconf.ContainsKey("Rezka"))
                    {
                        bwaconf["Rezka"]["enable"] = false;
                    }
                    else
                    {
                        bwaconf["Rezka"] = new JObject()
                        {
                            ["enable"] = false
                        };
                    }
                }
                else
                {
                    bwaconf["Rezka"] = new JObject()
                    {
                        ["enable"] = true,
                        ["cookie"] = cookie
                    };
                }

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion

        #region GetsTV
        [HttpGet]
        [Route("/bind/getstv")]
        async public Task<ActionResult> GetsTV(string user_id, string login, string pass)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                string body = $@"<div class='title'>Введите данные аккаунта <a href='https://getstv.com/user' target='_blank'>getstv.com</a></div>
                    <form method='get' action='/bind/getstv'>
                        <input type='hidden' name='user_id' value='{user_id}'>
                        <div class='form-group'>
                            <input type='text' name='login' placeholder='Email' required>
                        </div>
                        <div class='form-group'>
                            <input type='text' name='pass' placeholder='Пароль' required>
                        </div>
                        <button type='submit' class='button'>Добавить устройство</button>
                    </form>";

                return renderHtml("Привязка GetsTV", body);
            }
            else
            {
                if (string.IsNullOrEmpty(user_id))
                    return ContentTo("ошибка, отсутствует параметр user_id");

                string postdata = $"{{\"email\":\"{login}\",\"password\":\"{pass}\",\"fingerprint\":\"{CrypTo.md5(DateTime.Now.ToString())}\",\"device\":{{}}}}";
                var result = await Http.Post<JObject>($"{AppInit.conf.GetsTV.corsHost()}/api/login", new System.Net.Http.StringContent(postdata, Encoding.UTF8, "application/json"), headers: httpHeaders(AppInit.conf.GetsTV));

                if (result == null)
                    return ContentTo($"{AppInit.conf.GetsTV.corsHost()} недоступен, повторите попытку позже");

                string token = result.Value<string>("token");
                if (string.IsNullOrEmpty(token))
                    return ContentTo(JsonConvert.SerializeObject(result, Formatting.Indented));

                var bwaconf = loadconf(user_id);
                bwaconf["GetsTV"] = new JObject()
                {
                    ["enable"] = true,
                    ["token"] = token
                };

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion

        #region IptvOnline
        [HttpGet]
        [Route("/bind/iptvonline")]
        public ActionResult IptvOnline(string user_id, string login, string pass)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                string body = $@"<div class='title'>Введите данные <a href='https://iptv.online/ru/dealers/api' target='_blank'>https://iptv.online/ru/dealers/api</a></div>
                    <form method='get' action='/bind/iptvonline'>
                        <input type='hidden' name='user_id' value='{user_id}'>
                        <div class='form-group'>
                            <input type='text' name='login' placeholder='X-API-KEY' required>
                        </div>
                        <div class='form-group'>
                            <input type='text' name='pass' placeholder='X-API-ID' required>
                        </div>
                        <button type='submit' class='button'>Добавить устройство</button>
                    </form>";

                return renderHtml("Привязка iptv.online", body);
            }
            else
            {
                if (string.IsNullOrEmpty(user_id))
                    return ContentTo("ошибка, отсутствует параметр user_id");

                var bwaconf = loadconf(user_id);
                bwaconf["IptvOnline"] = new JObject()
                {
                    ["enable"] = true,
                    ["token"] = $"{login}:{pass}"
                };

                saveBind(user_id, bwaconf);
                return Redirect($"{host}/kit?user_id={HttpUtility.UrlEncode(user_id)}");
            }
        }
        #endregion


        #region getCookie
        async ValueTask<string?> getCookieRezka(string login, string passwd)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(passwd))
                return null;

            try
            {
                var clientHandler = new System.Net.Http.HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };

                clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new System.Net.Http.HttpClient(clientHandler))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

                    var postParams = new Dictionary<string, string>
                    {
                        { "login_name", login },
                        { "login_password", passwd },
                        { "login_not_save", "0" }
                    };

                    using (var postContent = new System.Net.Http.FormUrlEncodedContent(postParams))
                    {
                        using (var response = await client.PostAsync($"{AppInit.conf.Rezka.host}/ajax/login/", postContent))
                        {
                            if (response.Headers.TryGetValues("Set-Cookie", out var cook))
                            {
                                string cookie = string.Empty;

                                foreach (string line in cook)
                                {
                                    if (string.IsNullOrEmpty(line))
                                        continue;

                                    if (line.Contains("=deleted;"))
                                        continue;

                                    if (line.Contains("dle_user_id") || line.Contains("dle_password"))
                                        cookie += $"{line.Split(";")[0]}; ";
                                }

                                if (cookie.Contains("dle_user_id") && cookie.Contains("dle_password"))
                                    return Regex.Replace(cookie.Trim(), ";$", "");
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }
        #endregion
    }
}