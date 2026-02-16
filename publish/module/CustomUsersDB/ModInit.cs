using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shared.Models.Module;

namespace CustomUsersDB
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
                        if (File.Exists($"{conf.path}/users.txt"))
                            FileDB.Load(conf, "users.txt");    // кастомный вариант
                        else
                            FileDB.Load(conf, "users.json");   // массив accsdb.users в json

                        // await HttpAPI.Load();
                    }
                    catch { }

                    await Task.Delay(10_000); // 10 секунд между проверкой
                }
            });
        }
    }
}
