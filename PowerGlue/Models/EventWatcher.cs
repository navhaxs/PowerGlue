using FileLock;
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace PowerGlue.Models
{
    public partial class EventWatcher : Form
    {

        public static bool isRunning()
        {
            var fileLock = SimpleFileLock.Create(Constants.PIDLOCK_PATH);
            return (!fileLock.TestLockIsFree());
        }

        private SimpleFileLock fileLock = null;

        public EventWatcher(SimpleFileLock simpleFileLock) : this()
        {
            fileLock = simpleFileLock;
        }

        public EventWatcher()
        {
            InitializeComponent();

            var fileLock = SimpleFileLock.Create(Constants.PIDLOCK_PATH);
            if (!fileLock.TryAcquireLock())
            {
                Close();
            } else
            {
                SystemEvents.PowerModeChanged += OnPowerChange;
                this.FormClosed += EventWatcher_FormClosed;
            }
        }

        private void EventWatcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (fileLock != null)
            {
                fileLock.ReleaseLock();
            }
        }

        private DateTime lastUpdate;

        #region "hide winform"
        // a winform is requried to receive WndProc messages, but we want to hide the actual winform itself
        // https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }

        #endregion

        #region "event listeners"
        // Ensure that the current state is always up to date

        // Listen for WM_DISPLAYCHANGE event
        protected override void WndProc(ref Message m)
        {
            const uint WM_DISPLAYCHANGE = 0x007e;

            switch ((uint)m.Msg)
            {
                case WM_DISPLAYCHANGE:
                    onMonitorStateChange();
                    break;
            }

            base.WndProc(ref m);
        }

        // Listen for PowerMode change event
        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    onMonitorStateChange();
                    break;
            }
        }

        #endregion

        #region "event handler"
        //
        private void onMonitorStateChange()
        {
            // hack: replace with an event debouncer
            System.Threading.Thread.Sleep(2000);
            PowerGlue.Program.Run();
        }
        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                string title = $"PowerPoint will output on display \"{Config.LoadConfig().GetShortName()}\"\n\nLaunch the PowerGlue UI to change the output display";
                //title = (title.Length >= 64) ? title.Substring(0, 60) + "..." : title;
                notifyIcon1.BalloonTipText = title;
                notifyIcon1.ShowBalloonTip(30);
            }
        }
    }
}
