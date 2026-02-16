using System;
using System.Threading;
using Shared.Models.Module;
using Telegram.Bot;

namespace TelegramBot
{
    public class ModInit
    {
        public static void loaded(InitspaceModel conf)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    using var cts = new CancellationTokenSource();
                    var bot = new TelegramBotClient("YOUR_BOT_TOKEN", cancellationToken: cts.Token);

                    var me = await bot.GetMe();
                    Console.WriteLine($"\n\t@{me.Username} is running...\n\n");
                }
                catch (Exception ex) { Console.WriteLine("TelegramBot Exception: " + ex + "\n\n"); }
            });
        }
    }
}
