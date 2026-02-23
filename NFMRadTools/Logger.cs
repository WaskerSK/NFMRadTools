using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    public static class Logger
    {
        private static bool SupportsColors = OsSupportsColors();
        public static void Info(string msg) 
        {
            if (SupportsColors)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            Console.Write("[Info]: ");
            Console.WriteLine(msg);
            if (SupportsColors)
            {
                Console.ResetColor();
            }
        }
        public static void Warning(string msg) 
        {
            if (SupportsColors)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.Write("[Warning]: ");
            Console.WriteLine(msg);
            if (SupportsColors)
            {
                Console.ResetColor();
            }
        }
        public static void Error(string msg) 
        {
            if(SupportsColors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write("[Error]: ");
            Console.WriteLine(msg);
            if(SupportsColors)
            {
                Console.ResetColor();
            }
        }
        public static void Log(string msg, ConsoleColor? color = null)
        {
            if(SupportsColors && color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.WriteLine(msg);
            if(SupportsColors)
            {
                Console.ResetColor();
            }
        }
        private static bool OsSupportsColors()
        {
            return !(OperatingSystem.IsAndroid() || OperatingSystem.IsBrowser() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS());
        }
    }
}
