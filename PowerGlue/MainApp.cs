using Microsoft.Win32;
using StartupHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsDisplayAPI;

namespace PowerGlue
{
    public partial class MainApp : Form
    {
        static internal void applyConfig(string target_match)
        {
            //
            // Main logic for applying configuration:
            //

            // Get displays
            var displays = Display.GetDisplays();

            // Attempt to match
            var m = displays.Where(d => d.ToString().Contains(target_match)).First();

            // Write it to powerpoint's registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\16.0\PowerPoint\Options", true);
            key.SetValue("DisplayMonitor", m.DisplayName);
        }

        public const string RUN_ARGS = "--startup";
        public static string INI_PATH = Path.Combine(Environment.CurrentDirectory, "powerglue.ini");

        StartupManager StartupController = new StartupManager("PowerGlue", RegistrationScope.Local, false);

        IEnumerable<Display> displays;
        string SelectedDisplayName;

        public MainApp()
        {
            InitializeComponent();
            updateUI();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked) {
                StartupController.Register();
            } else {
                StartupController.Unregister();
            }
            updateUI();
        }

        private void updateUI()
        {
            checkBox1.Checked = !StartupController.IsRegistered;
            labelAutostartStatus.ForeColor = checkBox1.Checked ? Color.Red : Color.ForestGreen;
            labelAutostartStatus.Text = checkBox1.Checked ? "Apply on start-up is disabled" : "Apply on start-up is enabled";
            checkBox1.Text = checkBox1.Checked ? "Click to enable" : "Click to disable";
        }

        private void MainApp_Load(object sender, EventArgs e)
        {
            displays = Display.GetDisplays();
            displays.ToList().ForEach(x => listBox1.Items.Add(x.DeviceName));

            // Load config
            IniFile ini = new IniFile(Path.Combine(Environment.CurrentDirectory, "powerglue.ini"));
            var target_match = ini.IniReadValue("PowerPointDisplayMonitor", "Match"); // e.g. "DELL U2515H(DisplayPort)"

            // This will write the ini
            listBox1.SelectedItem = target_match;

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelDisplayName.Text = SelectedDisplayName = displays.ToList()[listBox1.SelectedIndex].DisplayName;

            var target_match = displays.ToList()[listBox1.SelectedIndex].DeviceName;

            // Write config
            IniFile ini = new IniFile(MainApp.INI_PATH);
            ini.IniWriteValue("PowerPointDisplayMonitor", "Match", target_match);

            applyConfig(target_match);
        }
    }
}
