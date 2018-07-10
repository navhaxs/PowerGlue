using PowerGlue.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PowerGlue
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            bool IsArgsHandled = false;
            if (args.Contains(Constants.AUTOSTART_ARG))
            {
                IsArgsHandled = true;

                try
                {
                    Run();
                } catch (Exception e) {
                    return -1;
                }
                return 1;
            }

            if (args.Contains(Constants.MONITOR_ARG))
            {
                IsArgsHandled = true;
                Application.Run(new EventWatcher());
            }


            if (!IsArgsHandled)
            {
                loadForm();
            }


            return 1;
        }

        /*
         * Main logic. Run this to apply the powerpoint registry hack.
         */
        static public void Run()
        {
            // Load config
            DisplayMeta res = Config.LoadConfig();
            var display = DisplayHelper.LookupFromMatch(res);
            if (display == null)
            {
                // @todo log this event: Apply fail on autostart
                return;
            }
            PowerPointRegistry.applyConfig(display.DisplayName);
        }

        static private void loadForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainApp());
        }

        // Log errors onto the Desktop
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            IniFile ini = new IniFile(Path.Combine(Environment.CurrentDirectory, "powerglue.ini"));
            var save_path = ini.IniReadValue("PowerGlue", "LogPath");
            var fileName = $"Error_{DateTime.Now:yyyyMMdd_hhmmss}.txt";

            Exception ex = (Exception)e.ExceptionObject;
            try
            {
                writeErrorlog(Path.Combine(save_path, fileName), ex);
            } catch
            {
                writeErrorlog(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName), ex);
            }
        }

        private static void writeErrorlog(String filePath, Exception e)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // Dump the error message
                writer.WriteLine("PowerGlue" + Environment.NewLine + "A tool to lockdown PowerPoint output monitor configuration. Author: Jeremy Wong 2018." + Environment.NewLine + "Unhandled error. Message :" + e.Message + "<br/>" + Environment.NewLine + "StackTrace :" + e.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
    }

}
