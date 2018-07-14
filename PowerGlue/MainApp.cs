using PowerGlue.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PowerGlue
{
    public partial class MainApp : Form
    {       

        StartupModel startup = new StartupModel();

        Dictionary<string, DisplayMeta> displays;
        string SelectedDevicePath;

        private DisplayMeta meta;

        public MainApp()
        {
            InitializeComponent();
            UpdateUI();
            UpdateMonitorStatus();
        }
        private void UpdateMonitorStatus()
        {
            labelDetectionServiceRunning.ForeColor = EventWatcher.isRunning() ? Color.ForestGreen : Color.Red;
            labelDetectionServiceRunning.Text = "Event watcher is " + (EventWatcher.isRunning() ? "running" : "not running");
        }

        private void UpdateUI()
        {
            checkBoxAutostartStatus.Checked = startup.IsRegistered();
            labelAutostartStatus.ForeColor = checkBoxAutostartStatus.Checked ? Color.ForestGreen : Color.Red;
            labelAutostartStatus.Text = "Apply on start-up is " + (checkBoxAutostartStatus.Checked ? "enabled" : "disabled");

            checkBoxDetectionServiceRunning.Checked = Config.GetWacherEnabled();

            checkBoxDetectionServiceRunning.Enabled = checkBoxAutostartStatus.Checked;
        }

        private void MainApp_Load(object sender, EventArgs e)
        {
            var config = Config.LoadConfig();
            var display = DisplayHelper.LookupFromMatch(config);

            displays = DisplayHelper.GetDisplays();
            foreach (var entry in displays)
            {
                var x = entry.Value.GetDisplayName();
                var index = listBox1.Items.Add(x);

                if (display != null && entry.Value.DevicePath == display.DevicePath)
                {
                    listBox1.SelectedIndex = index;
                }
            }


            // default initialize checklist with values from config
            redrawChecklist(config);
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

        private void checkBoxDetectionServiceRunning_CheckedChanged(object sender, EventArgs e)
        {
            Config.SetWacherEnabled(checkBoxDetectionServiceRunning.Checked);
            UpdateUI();
        }

        private void checkBoxAutostartStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutostartStatus.Checked)
            {
                startup.Register();
            }
            else
            {
                startup.Unregister();
            }
            UpdateUI();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = Constants.MONITOR_ARG;
            Info.FileName = Application.ExecutablePath;
            Process.Start(Info);

            UpdateUI();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateMonitorStatus();
        }
    }
}
