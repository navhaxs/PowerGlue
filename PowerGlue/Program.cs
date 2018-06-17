using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsDisplayAPI;

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

            if (args.Length == 1 && args[0].ToLower() == MainApp.RUN_ARGS)
            {
                // Load config
                IniFile ini = new IniFile(Path.Combine(Environment.CurrentDirectory, "powerglue.ini"));
                var target_match = ini.IniReadValue("PowerPointDisplayMonitor", "Match"); // e.g. "DELL U2515H(DisplayPort)"

                if (target_match != "") {
                    MainApp.applyConfig(target_match);
                    return 1;
                }
            }

            loadForm();
            return 1;
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
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Error_{DateTime.Now:yyyyMMdd_hhmmss}.txt");

            Exception ex = (Exception)e.ExceptionObject;
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // Dump the error message
                writer.WriteLine("PowerGlue" + Environment.NewLine + "A tool to lockdown PowerPoint output monitor configuration." + Environment.NewLine + "(c) Jeremy Wong 2018." + Environment.NewLine + "Unhandled error. Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
    }

}
