using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models;
using System.Threading.Tasks;

namespace Dynamic
{
    public class Middlewares
    {
        async public static Task<bool> InvokeAsync(bool first, HttpContext httpContext, IMemoryCache memoryCache)
        {
            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (first || requestInfo.IsLocalRequest || requestInfo.IsAnonymousRequest)
                return true;

            if (httpContext.Request.Path.Value.StartsWith("/king"))
            {
                await httpContext.Response.WriteAsync("kong dynamic");
                return false;
            }

            return true;
        }
    }
}
