using PowerGlue.Models;
using PowerGlue.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerGlue
{
    static class Program
    {

        public static bool SilentMode = false;
        public static bool TraceMode = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        
            List<string> argsList = new List<string>(args);
            // @jeremy TODO load from config as well
            SilentMode = args.Contains(Constants.SILENT_ARG);
            TraceMode = args.Contains(Constants.TRACE_ARG);

            if (argsList.Contains(Constants.AUTOSTART_ARG))
            {
                argsList.Add((Config.GetWacherEnabled()) ? Constants.MONITOR_ARG : Constants.ONCE_ARG);
            }

            // Determine what mode to run the tool
            if (argsList.Contains(Constants.MONITOR_ARG))
            {
                // Start the monitor service
                Application.Run(new EventWatcher(EventWatcher.RUN_MODE.MONITOR));
            }
            else if (argsList.Contains(Constants.ONCE_ARG))
            {
                // Run the monitor service if flag specified, regardless of config
                if (SilentMode)
                {
                    // Run silently
                    Run();
                }
                else
                {
                    // Run with Form wrapper to show balloon message
                    Application.Run(new EventWatcher(EventWatcher.RUN_MODE.ONCE));
                }
            } else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ConfigForm());
            }

            return 1;
        }
 
        // Main logic
        static public ApplyResult Run()
        {
            // Load config
            DisplayMeta res = Config.LoadConfig();

            // Scan for the target display (e.g. the projector) to see if it is attached to the machine
            var display = DisplayHelper.LookupFromMatch(res);
            if (display == null)
            {
                // It's not.
                return ApplyResult.Fail_NotDetected;
            }

            // Set PowerPoint to use that display using the up-to-date "DisplayName"
            if (PowerPointRegistry.ApplyConfig(display.DisplayName))
            {
                return ApplyResult.Success_WriteOK;
            } else
            {
                return ApplyResult.Success_NoWriteNeeded;
            }
        }        

        // Log errors to a file in the current directory
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logging.DumpError(ex);
        }
    }
}
