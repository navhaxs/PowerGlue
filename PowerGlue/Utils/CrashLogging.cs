using System;
using System.IO;

namespace PowerGlue.Utils
{
    public static class CrashLogging
    {
        public static void DumpError(Exception ex)
        {
            IniFile ini = new IniFile(Path.Combine(Environment.CurrentDirectory, "powerglue.ini"));
            var save_path = ini.IniReadValue("PowerGlue", "LogPath");
            var fileName = $"Error_{DateTime.Now:yyyyMMdd_hhmmss}.txt";

            try
            {
                Utils.CrashLogging.DumpErrorToFile(Path.Combine(save_path, fileName), ex);
            }
            catch
            {
                Utils.CrashLogging.DumpErrorToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName), ex);
            }
        }

        private static void DumpErrorToFile(String filePath, Exception e)
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
