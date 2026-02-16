using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models;
using Shared.Models.Base;
using Shared.Models.Module;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Lamson
{
    public class OnlineApi
    {
        public static List<(string, string, string, int)> Invoke(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, OnlineEventsModel args)
        {
            var online = new List<(string name, string url, string plugin, int index)>();

            void send(BaseSettings init, string plugin)
            {
                if (init.enable && !init.rip)
                {
                    string url = init.overridehost;
                    if (string.IsNullOrEmpty(url))
                        url = $"{host}/{plugin}";

                    online.Add((init.displayname ?? init.plugin, url, plugin, online.Count));
                }
            }

            send(ModInit.KinoGram, "kinogram");
            //send("KinoGram 2", ModInit.KinoGram, "ramkino");

            return online;
        }


        //async public static Task<List<(string, string, string, int)>> InvokeAsync(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, OnlineEventsModel args)
        //{
        //    // Асинхронный Events
        //}


        public static List<(string name, string url, int index)> Spider(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, OnlineSpiderModel args)
        {
            var online = new List<(string name, string url, int index)>();

            void send(BaseSettings init, string plugin)
            {
                if (init.spider && init.enable && !init.rip)
                {
                    string url = init.overridehost;
                    if (string.IsNullOrEmpty(url))
                        url = $"{host}/{plugin}";

                    online.Add((init.displayname ?? init.plugin, $"{url}?title={HttpUtility.UrlEncode(args.title)}&clarification=1&rjson=true&similar=true", online.Count));
                }
            }

            if (!args.isanime)
                send(ModInit.KinoGram, "kinogram");

            return online;
        }


        //async public static Task<List<(string name, string url, int index)>> SpiderAsync(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, OnlineSpiderModel args)
        //{
        //    // Асинхронный Spider
        //}
    }
}
