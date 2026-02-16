using Shared.Models.Module;
using Shared.Models.Online.Settings;

/// <summary>
/// версия 25.08.2025
/// lampac >= 147.1
/// online.js >= 1.4.4
/// </summary>
namespace Durex
{
    /// <summary>
    /// Получение учетной записи
    /// 
    /// tg: @monk_in_a_hat
    /// email: helpdesk@lumex.ink
    /// </summary>
    public class ModInit
    {
        public static string username => "";

        public static string password => "";
		
        /// <summary>
        /// Без протокола http, например movielab.one
        /// Либо username
        /// </summary>
		public static string domain => "";

        public static string vast_msg => "Реклама от VideoCDN";

        /// <summary>
        /// Лог запросов
        /// </summary>
        public static bool log => false;


        public static LumexSettings vsdn;

        /// <summary>
        /// Не забудьте включить мод в manifest.json
        /// </summary>
        public static void loaded(InitspaceModel conf)
        {
            /// <summary>
            /// https://portal.lumex.host/user/?do=settings
            /// </summary>
            vsdn = new LumexSettings("VideoCDN", "https://api.lumex.site", "API-токен", "https://portal.lumex.host", "ID клиент")
            {
                rhub = true, // проверка на парсер
                displayindex = 1,
                displayname = "VideoCDN",
                scheme = "http", // https если сайт за резинкой
                hls = false      // true - m3u8, false - mp4
            };
        }
    }
}
