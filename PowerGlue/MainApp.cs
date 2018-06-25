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
       
        public const string RUN_ARGS = "--startup";
        public static string INI_PATH = Path.Combine(Environment.CurrentDirectory, "powerglue.ini");

        StartupManager StartupController = new StartupManager("PowerGlue", RegistrationScope.Local, false);

        Dictionary<string, DisplayMeta> displays;
        string SelectedDevicePath;

        private DisplayMeta meta;

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
                var x = entry.Value.GetDisplayName();
                listBox1.Items.Add(x);
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
            if (listBox1.SelectedIndex < 0)
                return;

            SelectedDevicePath = displays.ToList()[listBox1.SelectedIndex].Key;

            var display = DisplayHelper.LookupFromPath(SelectedDevicePath);
            if (display == null)
                return;
            var displayDevice = DisplayHelper.GetMeta(display);

            label9.Text = $"PowerPoint is set to show the output on display monitor \'{display.DisplayName}\'";
            
            redrawChecklist(displayDevice);

            Config.WriteConfig(displayDevice);
        
            PowerPointRegistry.applyConfig(display.DisplayName);
        }

        private void redrawChecklist(DisplayMeta x)
        {
            checkedListBox1.Items.Clear();

            checkedListBox1.Items.Add($"[Device Name] {x.DeviceName}", !String.IsNullOrEmpty(x.DeviceName));
            checkedListBox1.Items.Add($"[Friendly Name] {x.FriendlyName}", !String.IsNullOrEmpty(x.FriendlyName));
            checkedListBox1.Items.Add($"[EDID Manufacture Code] {x.EDIDManufactureCode}", !String.IsNullOrEmpty(x.EDIDManufactureCode));
            checkedListBox1.Items.Add($"[EDID Manufacture Id] {x.EDIDManufactureId}", x.EDIDManufactureId != null);
            checkedListBox1.Items.Add($"[EDID Product Code] {x.EDIDProductCode}", x.EDIDProductCode != null);
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            if (checkedListBox1.SelectedIndex < 0)
            {
                return;
            }

            var display = DisplayHelper.LookupFromPath(SelectedDevicePath);
            if (display == null)
                return;
            var displayDevice = DisplayHelper.GetMeta(display);

            redrawChecklist(displayDevice);

            Config.WriteConfig(displayDevice);

            PowerPointRegistry.applyConfig(display.DisplayName);
        }
    }
}
