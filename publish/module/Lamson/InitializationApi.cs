using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models.Module;
using Shared.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Base;

namespace Lamson
{
    public class InitializationApi
    {
        public static ActionResult Invoke(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, InitializationModel args)
        {
            // авто зеркало
            if (args.init.plugin == "Rezka" && args.init.rhub && args.init.host == null)
                args.init.host = requestInfo.Country == "RU" ? "https://rezka.fi" : "https://hdrezka.me";

            if (requestInfo.Country != "RU" && httpContext.Request.Query["rchtype"].ToString() == "apk")
            {
                // у тебя андроид, хера ты забыл на сервере ? 
                if (args.init.plugin is "PornHub" or "Xhamster" or "Chaturbate")
                {
                    args.init.rhub = true; // парси в apk
                    args.init.rhub_fallback = false; // иди нахуй
                }
            }

            if (Regex.IsMatch(httpContext.Request.Headers.Referer.ToString(), "(prisma|lampishe)\\."))
            {
                httpContext.Response.StatusCode = 403;
                return new ContentResult
                {
                    Content = "Кто ты, воин?"
                };
            }

            if (requestInfo.user == null || 1 >= requestInfo.user.group)
            {
                args.init.vast = new VastConf()
                {
                    url = "http://vast.com/reklama.xml",
                    msg = "Якорь мне в жопу"
                };
            }

            // подмена запроса
            if (httpContext.Request.Path.Value.StartsWith("/chu/potok"))
                return new RedirectResult("/chaturbate-fix/potok" + httpContext.Request.QueryString.Value);

            // Mega Premum VIP Plus
            if (requestInfo.user != null && requestInfo.user.group > 1)
            {
                if (args.init.plugin is "Animebesst" or "AnilibriaOnline" or "Animevost")
                {
                    args.init.rhub = false;    // сейчас мы за тебя все сделаем
                    args.init.useproxy = true; // тут можно и проксю заюзать
                    args.init.proxy = new ProxySettings()
                    {
                        url = "https://asocks-list.org/userid.txt?type=res&country=UA"
                    };
                }
            }

            if (host.Contains("skaz.tv"))
                return new JsonResult(new { accsdb = true, msg = "Лена где сиськи ?, второй год ждем" });

            // не прерывать запрос
            return null;
        }


        //async public static Task<ActionResult> InvokeAsync(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, InitializationModel args)
        //{
        //	Асинхронный Invoke
        //}
    }
}
