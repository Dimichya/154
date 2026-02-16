using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Engine;
using Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogUserRequest
{
    public class Middlewares
    {
        public static ConcurrentQueue<(LogModelSql jurnal, UserInfoModelSql unfo, HeaderModelSql header)> queue = new();

        public static bool Invoke(bool first, HttpContext httpContext, IMemoryCache memoryCache)
        {
            if (first)
                return true;

            if (Regex.IsMatch(httpContext.Request.Path.Value, "^/(on/|(lite|online|sisi|timecode|sync|tmdbproxy|dlna|ts|tracks|backup|invc-ws)/js/|([^/]+/)?app\\.min\\.js|([^/]+/)?css/app\\.css|[a-zA-Z\\-]+\\.js)", RegexOptions.IgnoreCase))
                return true;

            string uri = httpContext.Request.Path.Value + httpContext.Request.QueryString.Value;
            if (uri.EndsWith("/personal.lampa") || Regex.IsMatch(uri, "^/(logrequest|reqinfo|rch/|lite/(events|withsearch)|lifeevents|externalids|storage|timecode)", RegexOptions.IgnoreCase))
                return true;

            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (requestInfo.IsLocalRequest || string.IsNullOrEmpty(requestInfo.user_uid))
                return true;

            if (ModInit.conf.only_authorized && requestInfo.user == null)
                return true;

            var unfo = new UserInfoModelSql
            {
                Id = CrypTo.md5($"{requestInfo.IP}:{requestInfo.Country}:{requestInfo.UserAgent}"),
                IP = requestInfo.IP,
                Country = requestInfo.Country,
                UserAgent = requestInfo.UserAgent
            };

            var header = new HeaderModelSql
            {
                Headers = httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            header.Id = CrypTo.md5(header.HeadersJson);

            var jurnal = new LogModelSql
            {
                time = DateTime.Now,
                uri = uri,
                uid = requestInfo.user_uid
            };

            queue.Enqueue((jurnal, unfo, header));

            return true;
        }
    }
}
