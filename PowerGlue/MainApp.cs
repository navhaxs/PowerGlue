using Microsoft.Win32;
using PowerGlue.Models;
using StartupHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;

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

            // TODO
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
                throw new Exception(@"Did not detect PowerPoint registry key in HKCU\Software\Microsoft\Office\");
            }

            // Write it to powerpoint's registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path + @"\PowerPoint\Options", true);
            key.SetValue("DisplayMonitor", m.DisplayName);
        }

        public const string RUN_ARGS = "--startup";
        public static string INI_PATH = Path.Combine(Environment.CurrentDirectory, "powerglue.ini");

        StartupManager StartupController = new StartupManager("PowerGlue", RegistrationScope.Local, false);

        Dictionary<string, DisplayMeta> displays;
        string SelectedDevicePath;

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
            displays = DisplayHelper.GetDisplays();
            foreach (var entry in displays)
            {
                listBox1.Items.Add(entry.Value.GetDisplayName());
            }

            // Load config
            IniFile ini = new IniFile(Path.Combine(Environment.CurrentDirectory, "powerglue.ini"));
            var target_match = ini.IniReadValue("PowerPointDisplayMonitor", "Match"); // e.g. "DELL U2515H(DisplayPort)"

            // This will write the ini
            listBox1.SelectedItem = target_match;
        }


        private bool isLoading = true;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                return;
            }

            SelectedDevicePath = displays.ToList()[listBox1.SelectedIndex].Key;

            var display = DisplayHelper.LookupFromPath(SelectedDevicePath);
            if (display == null)
            {
                return;
            }

            string devicePath = display.DisplayName;

            label9.Text = $"PowerPoint is set to show the output on display monitor \'{devicePath}\'";
            checkedListBox1.Items.Clear();
            checkedListBox1.Items.Add($"[Device Name] {displays.ToList()[listBox1.SelectedIndex].Value.DeviceName}", !String.IsNullOrEmpty(displays.ToList()[listBox1.SelectedIndex].Value.DeviceName));
            checkedListBox1.Items.Add($"[Friendly Name] {displays.ToList()[listBox1.SelectedIndex].Value.FriendlyName}", !String.IsNullOrEmpty(displays.ToList()[listBox1.SelectedIndex].Value.FriendlyName));
            checkedListBox1.Items.Add($"[EDID Manufacture Code] {displays.ToList()[listBox1.SelectedIndex].Value.EDIDManufactureCode}", !String.IsNullOrEmpty(displays.ToList()[listBox1.SelectedIndex].Value.EDIDManufactureCode));
            checkedListBox1.Items.Add($"[EDID Manufacture Id] {displays.ToList()[listBox1.SelectedIndex].Value.EDIDManufactureId}", displays.ToList()[listBox1.SelectedIndex].Value.EDIDManufactureId != null);
            checkedListBox1.Items.Add($"[EDID Product Code] {displays.ToList()[listBox1.SelectedIndex].Value.EDIDProductCode}", displays.ToList()[listBox1.SelectedIndex].Value.EDIDProductCode != null);

            // Write config
            IniFile ini = new IniFile(MainApp.INI_PATH);
            ini.IniWriteValue("PowerPointDisplayMonitor", "Match", SelectedDevicePath);

            applyConfig(devicePath);
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            // Write config // TODOsa
            IniFile ini = new IniFile(MainApp.INI_PATH);
            ini.IniWriteValue("Match", "DeviceName", SelectedDevicePath);
            ini.IniWriteValue("Match", "FriendlyName", SelectedDevicePath);
            ini.IniWriteValue("Match", "EDIDManufactureCode", SelectedDevicePath);
            ini.IniWriteValue("Match", "EDIDManufactureId", SelectedDevicePath);
            ini.IniWriteValue("Match", "EDIDProductCode", SelectedDevicePath);
        }
    }
}
