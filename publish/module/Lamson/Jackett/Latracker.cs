using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using System;
using Shared;
using Shared.Models.JacRed;

namespace Lamson
{
    [Route("latracker/[action]")]
    public class Latracker : BaseController
    {
        async public static ValueTask parsePage(string host, ConcurrentBag<TorrentDetails> torrents, string query, string cat)
        {
            //Console.WriteLine("\n\n\n\nquery: " + query + "\n\n");

            for (int i = 0; i < 20; i++)
            {
                torrents.Add(new TorrentDetails()
                {
                    trackerName = "latracker",
                    types = new string[] { "multfilm", "multserial" },
                    url = "http://latracker.com/64856.html",
                    title = "Большой город / Big siti [2075, 4K HDR]",
                    sid = 12,
                    pir = 45,
                    sizeName = "34GB",
                    magnet = "magnet",
                    createTime = DateTime.UtcNow,
                    name = "Большой город",
                    originalname = "Big siti",
                    relased = 2075
                });
            }
        }
    }
}
