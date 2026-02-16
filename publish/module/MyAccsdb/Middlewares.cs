using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Web;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Shared.Engine;

namespace MyAccsdb
{
    public class Middlewares
    {
        /// <summary>
        /// Реализация своей авторизации на примере CUB Premium
        /// </summary>
        async public static Task<bool> InvokeAsync(HttpContext httpContext, IMemoryCache memoryCache)
        {
            var requestInfo = httpContext.Features.Get<RequestModel>();
            if (requestInfo.IsLocalRequest || requestInfo.IsAnonymousRequest)
                return true;
			
            // закрываем только онлайн
            if (!httpContext.Request.Path.Value.StartsWith("/lite/"))
                return true;
				
            // для авторизации можно использовать account_email, uid, token, box_max

            // в account_email по умолчанию почта cub.red
            string email = httpContext.Request.Query["account_email"];
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                await httpContext.Response.WriteAsJsonAsync(new { accsdb = true, msg = "Войдите в аккаунт cub.red" }, httpContext.RequestAborted);
                return false;
            }

            string memkey = $"users/cubpremium:ex:{email}";
            if (!memoryCache.TryGetValue(memkey, out DateTime ex))
            {
                var user = await Http.Get<JObject>("https://cub.red/api/users/find?email=" + HttpUtility.UrlEncode(email), timeoutSeconds: 8);
                if (user == null || !user.Value<bool>("secuses"))
                {
                    memoryCache.Set(memkey, DateTime.Today, DateTime.Now.AddMinutes(1));
                }
                else
                {
                    var expires = DateTimeOffset.FromUnixTimeMilliseconds(user.Value<long>("premium")).DateTime;
                    memoryCache.Set(memkey, expires, DateTime.Now > expires ? DateTime.Now.AddMinutes(1) : expires);
                }
            }

            if (DateTime.Now > ex)
            {
                await httpContext.Response.WriteAsJsonAsync(new { accsdb = true, msg = "Купите CUB Premium" }, httpContext.RequestAborted);
                return false;
            }

            // Пользователь авторизован и имеет CUB Premium 
            return true;
        }
    }
}
