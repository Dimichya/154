using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shared.Models.Module;
using Newtonsoft.Json;
using Shared.Models.Base;
using Shared;
using Microsoft.EntityFrameworkCore;

namespace MariaDB
{
    public class ModInit
    {
        public static void loaded(InitspaceModel conf)
        {
            ThreadPool.QueueUserWorkItem(async _ => 
            {
                while (true)
                {
                    try
                    {
                        using (var context = new MariaDBContext())
                        {
                            context.Database.EnsureCreated();

                            var users = new ConcurrentBag<AccsUser>();

                            foreach (var user in context.Users.AsNoTracking())
                            {
                                users.Add(new AccsUser()
                                {
                                    id = user.token,
                                    expires = DateTime.Today.AddYears(10)
                                });
                            }

                            AppInit.conf.accsdb.users = users;
                            File.WriteAllText("current.conf", JsonConvert.SerializeObject(AppInit.conf, Formatting.Indented));
                        }
                    }
                    catch { }

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }
    }
}
