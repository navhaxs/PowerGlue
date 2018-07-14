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

        public enum RUN_MODE
        {
            MONITOR,
            ONCE
        }

        private RUN_MODE runOnlyOnce;

        public EventWatcher(RUN_MODE mode)
        {
            InitializeComponent();

            this.runOnlyOnce = mode;

            if (mode == RUN_MODE.MONITOR)
            {
                var fileLock = SimpleFileLock.Create(Constants.PIDLOCK_PATH);
                if (!fileLock.TryAcquireLock())
                {
                    Close();
                    // Another instance of this program is already running on the machine/
                    // Silent exit
                    return;
                }

                SystemEvents.PowerModeChanged += OnPowerChange;
                this.FormClosed += EventWatcher_FormClosed;

            } else if (mode == RUN_MODE.ONCE)
            {
                doWork();
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
            doWork();
        }
        #endregion

        private void doWork()
        {
            if (Program.Run())
            {
                if (!Program.SilentMode) showBalloon($"Applied settings\n\nPowerPoint will output on display \"{Config.LoadConfig().GetShortName()}\"");
                FlashIcon(Constants.OK_ICON);
            }
            else
            {
                if (!Program.SilentMode) showBalloon($"Failed to apply settings\n\nDid not detect display \"{Config.LoadConfig().GetShortName()}\"");
                FlashIcon(Constants.NO_ICON);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                doWork();
            }
        }

        private void showBalloon(string title)
        {
            //title = (title.Length >= 64) ? title.Substring(0, 60) + "..." : title;
            notifyIcon1.BalloonTipText = title;
            notifyIcon1.ShowBalloonTip(30);
        }

        private void EventWatcher_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Constants.DEFAULT_ICON;
        }

        private int flashCounter;
        private System.Drawing.Icon icon;

        private void FlashIcon(System.Drawing.Icon icon)
        {

            if (iconFlashTimer.Enabled)
            {
                iconFlashTimer.Stop();
            }

            this.icon = icon;
            flashCounter = 0;

            iconFlashTimer.Start();
        }

        private void iconFlashTimer_Tick(object sender, EventArgs e)
        {
            flashCounter += 1;
            

            if (flashCounter > 6)
            {
                iconFlashTimer.Stop();
                notifyIcon1.Icon = Constants.DEFAULT_ICON;

                if (this.runOnlyOnce == RUN_MODE.ONCE)
                {
                    Close();
                }

                return;
            }

            notifyIcon1.Icon = (flashCounter % 2 == 0) ? icon : Constants.DEFAULT_ICON;
        }
    }
}
