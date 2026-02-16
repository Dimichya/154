using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Web;
using Shared;
using Shared.Engine;
using Shared.Models.SISI.Base;

namespace Lamson.Controllers
{
    public class PornGram : BaseSisiController
    {
        ProxyManager proxyManager = new ProxyManager(ModInit.PornGram);

        [HttpGet]
        [Route("porngram")]
        async public Task<ActionResult> Index(string search, string sort, int pg = 1)
        {
            if (!ModInit.PornGram.enable)
                return OnError("disabled");

            string memKey = $"porngram:list:{search}:{sort}:{pg}";
            if (!hybridCache.TryGetValue(memKey, out List<PlaylistItem> playlists))
            {
                var proxy = proxyManager.Get();
                // await http
                // пасинг

                playlists = new List<PlaylistItem>();

                for (int i = 0; i < 24; i++)
                {
                    playlists.Add(new PlaylistItem()
                    {
                        name = "Tomsk Theater Square",
                        video = $"{host}/porngram/video?href={HttpUtility.UrlEncode("https://www.elecard.com/ru/videos")}",
                        picture = HostImgProxy("https://www.elecard.com/storage/thumbs/1_1280x_FFFFFF/images/Video%20Previews/TheaterSquare_640x360.jpg"),
                        time = "9:15",
                        quality = "4K",
                        json = true
                    });
                }

                proxyManager.Success();
                hybridCache.Set(memKey, playlists, cacheTime(20));
            }

            var menu = new List<MenuItem>()
            {
                new MenuItem()
                {
                    title = "Поиск",
                    search_on = "search_on",
                    playlist_url = host + "/porngram",
                },
                new MenuItem()
                {
                    title = $"Сортировка: {(string.IsNullOrEmpty(sort) ? "новинки" : sort)}",
                    playlist_url = "submenu",
                    submenu = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            title = "Новинки",
                            playlist_url = host + "/porngram"
                        },
                        new MenuItem()
                        {
                            title = "Лучшее",
                            playlist_url = host + $"/porngram?sort=porno-online"
                        },
                        new MenuItem()
                        {
                            title = "Популярное",
                            playlist_url = host + $"/porngram?sort=xxx-top"
                        }
                    }
                }
            };

            return OnResult(playlists, menu);
        }


        [HttpGet]
        [Route("porngram/video")]
        async public Task<ActionResult> Video(string href)
        {
            if (!ModInit.PornGram.enable)
                return OnError("disabled");

            var proxy = proxyManager.Get();

            string memKey = $"porngram:video:{href}";
            if (!hybridCache.TryGetValue(memKey, out Dictionary<string, string> stream_links))
            {
                // await http
                // пасинг

                stream_links = new Dictionary<string, string>()
                {
                    ["2160p"] = "https://www.elecard.com/storage/video/TheaterSquare_3840x2160.mp4",
                    ["1080p"] = "https://www.elecard.com/storage/video/TheaterSquare_1920x1080.mp4",
                    ["720p"] = "https://www.elecard.com/storage/video/TheaterSquare_1280x720.mp4"
                };

                proxyManager.Success();
                hybridCache.Set(memKey, stream_links, cacheTime(10));
            }

            return OnResult(stream_links, ModInit.PornGram, proxy);
        }
    }
}
