using Shared.Models.Module;
using System;

namespace Dynamic
{
    public class ModInit
    {
        public static void loaded(InitspaceModel conf)
        {
            Console.WriteLine("Dynamic loaded");
        }

        public static void Dispose()
        {
            Console.WriteLine("Dynamic dispose");
        }
    }
}
