using Shared.Engine;
using Shared.Models.Module;
using Shared.Models.Online.Settings;
using Shared.Models.SISI.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lamson
{
    public class ModInit
    {
        public static OnlinesSettings KinoGram;

        public static SisiSettings PornGram;

        /// <summary>
        /// модуль загружен
        /// </summary>
        public static void loaded(InitspaceModel conf)
        {
            KinoGram = new OnlinesSettings("KinoGram", "kinogram.com", streamproxy: true);
			KinoGram = ModuleInvoke.Init("KinoGram", KinoGram);

            PornGram = new SisiSettings("PornGram", "porngram.com");
			PornGram = ModuleInvoke.Init("PornGram", PornGram);

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                // асинхронные задачи
                await Task.Delay(1000);
            });


            var timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds) 
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += async (s, e) => 
            {
                // cron
                await Task.Delay(1000);
            };
        }
    }
}
