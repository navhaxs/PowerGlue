using System;

namespace PowerGlue.Models
{
    public static class Constants
    {
        public static string PIDLOCK_PATH = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "powerglue.lock");
        public const string AUTOSTART_ARG = "--startup";
        public const string MONITOR_ARG = "--monitor";
        public static string INI_PATH = System.IO.Path.Combine(Environment.CurrentDirectory, "powerglue.ini");
    }
}
