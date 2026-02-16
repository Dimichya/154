using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Shared.Models;

namespace GeoBlocking
{
    public class Middlewares
    {
        public static bool Invoke(HttpContext httpContext, IMemoryCache memoryCache)
        {
            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (requestInfo.IsLocalRequest)
                return true;

            // Список разрешённых стран "UA, RU, BY, KZ" 
            bool allowCountry = requestInfo.Country is "UA" or "RU" or "BY" or "KZ";

            // Доступ разрешен для "UA, RU, BY, KZ" 
            return allowCountry; // Или "return !allowCountry;" для противоположного результата 
        }
    }
}
