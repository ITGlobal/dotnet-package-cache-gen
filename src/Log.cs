using System;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class Log
    {

        public static bool IsEnabled { get; set; }

        public static void WriteLine(string message)
        {
            if (IsEnabled)
            {
                Console.Error.WriteLine($"LOG: {message}");
            }
        }

    }
}