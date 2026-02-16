using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models.Module;
using Shared.Models;
using System.Collections.Generic;

namespace Durex
{
    public class OnlineApi
    {
        public static List<(string, string, string, int)> Invoke(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, OnlineEventsModel args)
        {
            return new List<(string name, string url, string plugin, int index)>() 
            {
                (ModInit.vsdn.displayname, $"{host}/vcdn", "vcdn", ModInit.vsdn.displayindex)
            };
        }
    }
}
