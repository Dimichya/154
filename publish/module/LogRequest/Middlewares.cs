using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text;
using System;
using Shared.Models;
using System.Text.Json;

namespace LogRequest
{
    public class Middlewares
    {
        static Middlewares()
        {
            Directory.CreateDirectory("cache/logs/request");
        }

        public static bool Invoke(HttpContext httpContext, IMemoryCache memoryCache)
        {
            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (requestInfo.IsLocalRequest || requestInfo.IsAnonymousRequest)
                return true;

            WriteLog(httpContext, requestInfo);
            return true;
        }


        static FileStream logFileStream = null;

        static void WriteLog(HttpContext httpContext, RequestModel requestInfo)
        {
            string uri = httpContext.Request.Path.Value + httpContext.Request.QueryString.Value;
            string clientip = httpContext.Connection.RemoteIpAddress.ToString();
            if (clientip.Contains("127.0.0.1") || uri.EndsWith("/personal.lampa"))
                return;

            string data = JsonSerializer.Serialize(new
            {
                time = DateTime.Now,
                uri,
                requestInfo.Country,
                requestInfo.IP,
                requestInfo.UserAgent
            });

            string dateLog = DateTime.Today.ToString("dd.MM.yy");
            string patchlog = $"cache/logs/request/{dateLog}.log";

            if (logFileStream == null || !File.Exists(patchlog))
                logFileStream = new FileStream(patchlog, FileMode.Append, FileAccess.Write);

            var buffer = Encoding.UTF8.GetBytes(data + "\n");
            logFileStream.Write(buffer, 0, buffer.Length);
            logFileStream.Flush();
        }
    }
}
