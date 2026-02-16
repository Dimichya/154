using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Models.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogUserRequest
{
    public class ModInit
    {
        #region static
        public static InitspaceModel init { get; set; }

        public static (int logDay, string apiKey, bool only_authorized) conf = default;

        public static object stats = null;

        static Timer _statsTimer, _clearJurnalTimer, _updateDbTimer;
        #endregion

        public static void loaded(InitspaceModel _conf)
        {
            init = _conf;

            using (var sqlDb = new AppDbContext())
                sqlDb.Database.EnsureCreated();

            var manifest = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(init.path, "manifest.json")));
            conf.only_authorized = manifest["only_authorized"]?.Value<bool>() ?? true;
            conf.logDay = manifest["logDay"]?.Value<int>() ?? 30;
            conf.apiKey = manifest["apiKey"]?.Value<string>();

            _clearJurnalTimer = new Timer(ClearJurnal, null, TimeSpan.FromMinutes(50), TimeSpan.FromDays(1));
            _statsTimer = new Timer(UpdateStats, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            _updateDbTimer = new Timer(UpdateDb, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }


        #region ClearJurnal
        static void ClearJurnal(object state)
        {
            try
            {
                using (var sqlDb = new AppDbContext())
                {
                    var now = DateTime.Now.AddDays(-conf.logDay);

                    sqlDb.jurnal
                         .AsNoTracking()
                         .Where(i => now > i.time)
                         .ExecuteDelete();

                    #region clear unfo
                    {
                        var isd = sqlDb.jurnal
                            .AsNoTracking()
                            .Select(i => i.unfo)
                            .ToHashSet();

                        var delete_ids = new HashSet<string>();

                        foreach (string id in sqlDb.unfo.AsNoTracking().Select(i => i.Id))
                        {
                            if (!isd.Contains(id))
                                delete_ids.Add(id);
                        }

                        sqlDb.unfo
                         .AsNoTracking()
                         .Where(i => delete_ids.Contains(i.Id))
                         .ExecuteDelete();

                        isd.Clear(); isd = null;
                        delete_ids.Clear(); delete_ids = null;
                    }
                    #endregion

                    #region clear header
                    {
                        var isd = sqlDb.jurnal
                            .AsNoTracking()
                            .Select(i => i.header)
                            .ToHashSet();

                        var delete_ids = new HashSet<string>();

                        foreach (string id in sqlDb.headers.AsNoTracking().Select(i => i.Id))
                        {
                            if (!isd.Contains(id))
                                delete_ids.Add(id);
                        }

                        sqlDb.headers
                          .AsNoTracking()
                          .Where(i => delete_ids.Contains(i.Id))
                          .ExecuteDelete();

                        isd.Clear(); isd = null;
                        delete_ids.Clear(); delete_ids = null;
                    }
                }
                #endregion
            }
            catch (Exception ex) { Console.WriteLine("LogUserRequest / ClearJurnal: " + ex); }
        }
        #endregion

        #region UpdateStats
        static bool updatingStats = false;
        static void UpdateStats(object state)
        {
            if (updatingStats)
                return;

            try
            {
                updatingStats = true;

                using (var sqlDb = new AppDbContext())
                {
                    var now = DateTimeOffset.Now;
                    var monthUsers = sqlDb.jurnal
                        .AsNoTracking()
                        .Where(u => u.time.Year == now.Year && u.time.Month == now.Month);

                    int today = monthUsers.Count(u => u.time.Day == now.Day);
                    int month = monthUsers.Count();

                    int uniqueUserAgent = sqlDb.unfo
                        .AsNoTracking()
                        .Select(i => i.UserAgent)
                        .Distinct()
                        .Count();

                    int uniqueIp = sqlDb.unfo
                        .AsNoTracking()
                        .Select(i => i.IP)
                        .Distinct()
                        .Count();

                    #region topUsers
                    var userCounts = new Dictionary<string, int>();

                    foreach (var user in monthUsers)
                    {
                        userCounts.TryGetValue(user.uid, out int count);
                        userCounts[user.uid] = count + 1;
                    }

                    var topUsers = userCounts
                        .OrderByDescending(x => x.Value)
                        .Take(20)
                        .Select(kv => new { uid = kv.Key, count = kv.Value })
                        .ToArray();
                    #endregion

                    #region topBalancers
                    var balancersCounts = new Dictionary<string, int>();

                    foreach (string uri in monthUsers.AsEnumerable().Select(u =>
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

                    stats = new
                    {
                        today,
                        month,
                        uniqueUserAgent,
                        uniqueIp,
                        topUsers,
                        topBalancers
                    };
                }
            }
            catch (Exception ex) { Console.WriteLine("LogUserRequest / UpdateStats: " + ex); }
            finally
            {
                updatingStats = false;
            }
        }
        #endregion

        #region UpdateDb
        static bool updatingDb = false;
        static void UpdateDb(object state)
        {
            if (updatingDb || updatingStats)
                return;

            try
            {
                updatingDb = true;

                using (var sqlDb = new AppDbContext())
                {
                    var jurnal = new List<(LogModelSql jurnal, UserInfoModelSql unfo, HeaderModelSql header)>(Middlewares.queue.Count);
                    while (Middlewares.queue.TryDequeue(out (LogModelSql jurnal, UserInfoModelSql unfo, HeaderModelSql header) _q))
                        jurnal.Add(_q);

                    foreach (var j in jurnal.OrderBy(i => i.jurnal.time))
                    {
                        if (sqlDb.unfo.Find(j.unfo.Id) == null)
                            sqlDb.unfo.Add(j.unfo);

                        if (sqlDb.headers.Find(j.header.Id) == null)
                            sqlDb.headers.Add(j.header);

                        sqlDb.jurnal.Add(new LogModelSql()
                        {
                            time = j.jurnal.time,
                            uri = j.jurnal.uri,
                            uid = j.jurnal.uid,
                            unfo = j.unfo.Id,
                            header = j.header.Id
                        });

                        sqlDb.SaveChanges();
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("LogUserRequest / UpdateDb: " + ex); }
            finally
            {
                updatingDb = false;
            }
        }
        #endregion
    }
}
