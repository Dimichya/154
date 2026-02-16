using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Engine;

namespace Lamson
{
    public class Middlewares
    {
        public static bool Invoke(bool first, HttpContext httpContext, IMemoryCache memoryCache)
        {
            if (Regex.IsMatch(httpContext.Request.Path.Value, "^/(kinogram|porngram|lamson)"))
                httpContext.Response.Headers.Add("X-Lamson", "true");

            return true;
        }

        async public static Task<bool> InvokeAsync(bool first, HttpContext httpContext, IMemoryCache memoryCache)
        {
            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (first || requestInfo.IsLocalRequest || requestInfo.IsAnonymousRequest)
                return true;

            if (httpContext.Request.Path.Value.StartsWith("/lamson"))
            {
                string token = Regex.Match(httpContext.Request.QueryString.Value, "(\\?|&)token=([^&]+)").Groups[2].Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    httpContext.Response.ContentType = "application/json; charset=utf-8";
                    await httpContext.Response.WriteAsync("[{\"error\":\"token == null\"}]");
                    return false;
                }
            }

            //bool isvip = (await Http.Get($"http://myapi.com/vip?token={token}")) == "OK";
            bool isvip = true;
            if (isvip)
                return true;

            return false;
        }
    }
}
