using System.Text.Json;
using IO = System.IO;
using System.Collections.Concurrent;
using Shared.Models.Module;
using Shared;
using Shared.Models.Base;

namespace UserAPI
{
    public class ModInit
    {
        public static string folder_mod { get; private set; }

        /// <summary>
        /// модуль загружен
        /// </summary>
        public static void loaded(InitspaceModel conf)
        {
            folder_mod = conf.path;

            if (IO.File.Exists($"{conf.path}/users.json"))
                AppInit.conf.accsdb.users = JsonSerializer.Deserialize<ConcurrentBag<AccsUser>>(IO.File.ReadAllText($"{conf.path}/users.json"));
        }
    }
}
