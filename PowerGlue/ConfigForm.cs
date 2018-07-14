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
    public partial class ConfigForm : Form
    {       

        LoginAutostart startup = new LoginAutostart();

        Dictionary<string, DisplayMeta> displays;
        string SelectedDevicePath;

        public ConfigForm()
        {
            InitializeComponent();
            UpdateUI();
            UpdateMonitorStatus();
        }
        private void UpdateMonitorStatus()
        {
            bool state = EventWatcher.IsRunning();
            labelDetectionServiceRunning.ForeColor = state ? Color.ForestGreen : Color.Red;
            labelDetectionServiceRunning.Text = "Event watcher is " + (state ? "running" : "not running");
            pictureBox3.Image = Bitmap.FromHicon(new Icon((state ? Constants.OK_ICON : Constants.NO_ICON), new Size(24, 24)).Handle);

        }

        private void UpdateUI()
        {
            checkBoxAutostartStatus.Checked = startup.IsRegistered();
            labelAutostartStatus.ForeColor = checkBoxAutostartStatus.Checked ? Color.ForestGreen : Color.Red;
            labelAutostartStatus.Text = "Apply on start-up is " + (checkBoxAutostartStatus.Checked ? "enabled" : "disabled");
            pictureBox2.Image = Bitmap.FromHicon(new Icon((checkBoxAutostartStatus.Checked ? Constants.OK_ICON : Constants.NO_ICON), new Size(24, 24)).Handle);

            checkBoxDetectionServiceRunning.Checked = Config.GetWacherEnabled();

            checkBoxDetectionServiceRunning.Enabled = checkBoxAutostartStatus.Checked;
        }

        private void MainApp_Load(object sender, EventArgs e)
        {
            Rescan();

            // default initialize checklist with values from config
            RedrawChecklist(Config.LoadConfig());
        }

        private void Rescan()
        {
            listBox1.Items.Clear();

            var display = DisplayHelper.LookupFromMatch(Config.LoadConfig());

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
        }

        private bool isLoading = true;
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                return;

            SelectedDevicePath = displays.ToList()[listBox1.SelectedIndex].Key;

            var display = DisplayHelper.LookupFromPath(SelectedDevicePath);
            if (display == null)
                return;
            var displayDevice = DisplayHelper.GetMeta(display);

            label9.Text = $"PowerPoint is set to show the output on display monitor \'{display.DisplayName}\'\nChanges are applied immediately";
            
            RedrawChecklist(displayDevice);

            Config.WriteConfig(displayDevice);
        
            PowerPointRegistry.ApplyConfig(display.DisplayName);
        }

        private void RedrawChecklist(DisplayMeta x)
        {
            checkedListBox1.Items.Clear();

            checkedListBox1.Items.Add($"[Device Name] {x.DeviceName}", !String.IsNullOrEmpty(x.DeviceName));
            checkedListBox1.Items.Add($"[Friendly Name] {x.FriendlyName}", !String.IsNullOrEmpty(x.FriendlyName));
            checkedListBox1.Items.Add($"[EDID Manufacture Code] {x.EDIDManufactureCode}", !String.IsNullOrEmpty(x.EDIDManufactureCode));
            checkedListBox1.Items.Add($"[EDID Manufacture Id] {x.EDIDManufactureId}", x.EDIDManufactureId != null);
            checkedListBox1.Items.Add($"[EDID Product Code] {x.EDIDProductCode}", x.EDIDProductCode != null);
        }

        private void CheckedListBox1_SelectedIndexChanged(object sender, EventArgs e)
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

            RedrawChecklist(displayDevice);

            Config.WriteConfig(displayDevice);

            PowerPointRegistry.ApplyConfig(display.DisplayName);
        }

        private void CheckBoxDetectionServiceRunning_CheckedChanged(object sender, EventArgs e)
        {
            Config.SetWacherEnabled(checkBoxDetectionServiceRunning.Checked);
            UpdateUI();
        }

        private void CheckBoxAutostartStatus_CheckedChanged(object sender, EventArgs e)
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

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo Info = new ProcessStartInfo
            {
                Arguments = Constants.MONITOR_ARG + (Program.SilentMode ? " " + Constants.SILENT_ARG : ""),
                FileName = Application.ExecutablePath
            };
            Process.Start(Info);

            UpdateUI();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateMonitorStatus();
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Rescan();
        }
    }
}
