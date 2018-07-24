using System;

namespace PowerGlue.Models
{
    public static class Constants
    {
        public static string PIDLOCK_PATH = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "powerglue.lock");
        public static string INI_PATH = System.IO.Path.Combine(Environment.CurrentDirectory, "powerglue.ini");
        public static System.Drawing.Icon DEFAULT_ICON = Properties.Resources.CSOfficePowerpoint2013Application;
        public static System.Drawing.Icon OK_ICON = Properties.Resources.StatusOK;
        public static System.Drawing.Icon NO_ICON = Properties.Resources.StatusNo_cyan;

        // Run mode flags
        public const string AUTOSTART_ARG = "--startup";
        public const string MONITOR_ARG = "--monitor";
        public const string ONCE_ARG = "--run";
       
        // Option flags
        public const string SILENT_ARG = "--no-balloon";
        public const string TRACE_ARG = "--trace";
    }
}
