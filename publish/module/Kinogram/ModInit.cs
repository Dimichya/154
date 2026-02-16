using Shared;
using Shared.Models.Online.Settings;

namespace Kinogram
{
    public class ModInit
    {
        public static OnlinesSettings KinoGram;

        /// <summary>
        /// модуль загружен
        /// </summary>
        public static void loaded()
        {
            KinoGram = new OnlinesSettings("KinoGram", "kinogram.com", streamproxy: true)
            {
                displayname = "KinoGram"
            };

            // Выводить "уточнить поиск"
            AppInit.conf.online.with_search.Add("kinogram");
        }
    }
}
