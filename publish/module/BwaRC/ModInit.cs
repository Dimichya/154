using System.IO;
using Shared.Models.Module;

namespace BwaRC
{
    public class ModInit
    {
        /// <summary>
        /// true - Клиент сам выполняет запросы к источнику 
        /// false - Ваш сервер выполняет запросы для клиента
        /// </summary>
        public static bool rhub_forced = false;

        public static string folder_mod { get; private set; } 


        /// <summary>
        /// Модуль загружен
        /// </summary>
        public static void loaded(InitspaceModel conf)
        {
            folder_mod = conf.path;
            Directory.CreateDirectory($"{folder_mod}/raw");
        }
    }
}
