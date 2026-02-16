using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUserRequest.Controllers
{
    public class ApiController : BaseController
    {
        [Route("logrequest")]
        public ActionResult Index()
        {
            string htmlPath = Path.Combine(ModInit.init.path, "index.html");
            return ContentTo(System.IO.File.ReadAllText(htmlPath));
        }


        [Route("logrequest/api")]
        public ActionResult Api(string apikey, string uid, int skip = 0, int take = 200)
        {
            if (apikey != ModInit.conf.apiKey)
                return Json(new { error = "access denied" });

            using (var sqlDb = new AppDbContext())
            {
                var query = sqlDb.jurnal.AsNoTracking();

                if (!string.IsNullOrEmpty(uid))
                    query = query.Where(j => j.uid == uid);

                var jurnal = query
                    .OrderByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToArray();

                if (jurnal.Length == 0)
                    return Json(new { error = "Empty" });

                return Json(jurnal.Select(j =>
                {
                    var unfo = j.unfo != null ? sqlDb.unfo.Find(j.unfo) : null;
                    var header = j.header != null ? sqlDb.headers.Find(j.header) : null;

                    return new
                    {
                        j.Id,
                        j.time,
                        j.uri,
                        user_uid = j.uid,
                        unfo?.IP,
                        unfo?.Country,
                        unfo?.UserAgent,
                        header?.Headers
                    };
                }).ToArray());
            }
        }


        [Route("logrequest/stats")]
        public ActionResult Stats(string apikey, string uid)
        {
            if (apikey != ModInit.conf.apiKey)
                return ContentTo("access denied");

            if (string.IsNullOrEmpty(uid))
                return Json(ModInit.stats ?? new { error = "null" });

            using (var sqlDb = new AppDbContext())
            {
                var jurnal = sqlDb.jurnal
                    .AsNoTracking()
                    .Where(j => j.uid == uid)
                    .ToArray();

                if (jurnal.Length == 0)
                    return Json(new { error = "Empty" });

                var now = DateTime.Now;

                var monthUser = jurnal
                    .Where(u => u.time.Month == now.Month)
                    .Select(i => new
                    {
                        i.time,
                        i.uid,
                        i.unfo,
                        i.uri
                    });

                int today = monthUser.Where(u => u.time.Day == now.Day).Count();
                int month = monthUser.Count();

                var unfo_ids = monthUser.Select(i => i.unfo).ToHashSet();

                var unfo = sqlDb.unfo
                    .AsNoTracking()
                    .Where(j => unfo_ids.Contains(j.Id))
                    .ToArray();

                int uniqueUserAgent = unfo.Select(i => i.UserAgent).Distinct().Count();
                int uniqueIp = unfo.Select(i => i.IP).Distinct().Count();

                #region topBalancers
                var balancersCounts = new Dictionary<string, int>();

                foreach (string uri in monthUser.Select(u =>
                {
                    if (u.uri != null && u.uri.StartsWith("/"))
                    {
                        if (u.uri.StartsWith("/lite/"))
                        {
                            var path = u.uri.Substring(6);
                            return path.Split('/', '?')[0];
                        }
                        else if (u.uri.StartsWith("/rc/"))
                        {
                            var path = u.uri.Substring(4);
                            return path.Split('/', '?')[0];
                        }

                        string balacer = u.uri.Split('/')[1].Split('?')[0];
                        if (balacer is "sisi" or "http:" or "https:")
                            return null;

                        return balacer;
                    }
                    return null;
                }))
                {
                    if (string.IsNullOrEmpty(uri))
                        continue;

                    balancersCounts.TryGetValue(uri, out int count);
                    balancersCounts[uri] = count + 1;
                }

                var topBalancers = balancersCounts
                    .OrderByDescending(x => x.Value)
                    .Take(50)
                    .Select(kv => new { balancer = kv.Key, count = kv.Value })
                    .ToArray();
                #endregion

                return Json(new
                {
                    today,
                    month,
                    uniqueUserAgent,
                    uniqueIp,
                    topUsers = new string[] { },
                    topBalancers
                });
            }
        }
    }
}
