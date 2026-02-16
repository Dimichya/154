using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Engine;
using Shared.Models.Base;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace CustomUsersDB
{
    public static class HttpAPI
    {
        async public static Task Load()
        {
            // Формат данных как в users.json
            var users = await Http.Get<ConcurrentBag<AccsUser>>("https://myhost-api.com/users");
            if (users != null)
            {
                AppInit.conf.accsdb.users = users;
                File.WriteAllText("current.conf", JsonConvert.SerializeObject(AppInit.conf, Formatting.Indented));
            }


            // Свой формат данных
            var users2 = await Http.Get<JArray>("https://myhost-api.com/users2"); 
            if (users2 != null)
            {
                _ = @"
[
  {
    ""token"": ""41fcbc35cdf40809a2d8cfbdaf91d7139279f5df747fb9d35783b25b32e8b2aa"",
    ""ex"": ""2035-02-15T00:00:00+02:00"",
    ""vip"": true
  },
  {
    ""token"": ""0809a2d8cfbdaf915df747fb9d3578341fcbc35cdf4b25b32e8b2aad7139279f"",
    ""ex"": ""2035-02-15T00:00:00+02:00"",
    ""vip"": false
  }
]
";

                var result = new ConcurrentBag<AccsUser>();

                foreach (var user in users2)
                {
                    if (!user.Value<bool>("vip"))
                        continue;

                    users.Add(new AccsUser()
                    {
                        id = user.Value<string>("token"),
                        expires = user.Value<DateTime>("ex")
                    });
                }

                AppInit.conf.accsdb.users = result;
                File.WriteAllText("current.conf", JsonConvert.SerializeObject(AppInit.conf, Formatting.Indented));
            }
        }
    }
}
