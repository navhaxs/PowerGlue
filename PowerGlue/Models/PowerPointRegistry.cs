using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows.Forms;
using WindowsDisplayAPI;

namespace PowerGlue.Models
{
    static class PowerPointRegistry
    {
        /*
         * Logic for writing PowerPoint display configuration
         */
        static internal bool ApplyConfig(string target_match)
        {
            // Get displays
            var displays = Display.GetDisplays();

            // Attempt to match
            var m = displays.Where(d => d.ToString().Contains(target_match)).First();
            string path = null;
            string[] try_paths = {
                @"Software\Microsoft\Office\16.0",
                @"Software\Microsoft\Office\15.0",
                @"Software\Microsoft\Office\14.0",
                @"Software\Microsoft\Office\12.0"
            };

            foreach (string i in try_paths)
            {
                if (Registry.CurrentUser.OpenSubKey(i) != null)
                {
                    path = i;
                }
            }

            if (path == null)
            {
                MessageBox.Show("This tool requires PowerPoint to be installed and run before for this user.");
                throw new Exception(@"Did not detect PowerPoint registry key in HKCU\Software\Microsoft\Office\");
            }

            // Write it to powerpoint's registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path + @"\PowerPoint\Options", true);

            if (key.GetValue("DisplayMonitor").ToString() == m.DisplayName)
            {
                return false;
            }

            key.SetValue("DisplayMonitor", m.DisplayName);

            return true;
        }
    }
}
