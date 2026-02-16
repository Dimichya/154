using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models.Module;
using Shared.Models;
using System.Collections.Generic;
using Shared.Models.SISI.Base;

namespace Lamson
{
    public class SisiApi
    {
        public static List<ChannelItem> Invoke(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, SisiEventsModel args)
        {
            return new List<ChannelItem>()
            {
                new ChannelItem("PornGram", $"{host}/porngram", 1),
                //new ChannelItem("TwoPorn", $"{host}/twoporn")
            };
        }


        //async public static Task<List<ChannelItem>> InvokeAsync(HttpContext httpContext, IMemoryCache memoryCache, RequestModel requestInfo, string host, SisiEventsModel args)
        //{
        //    // Асинхронный Events
        //}
    }
}
