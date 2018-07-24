using FileLock;
using Microsoft.Win32;
using PowerGlue.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerGlue.Models
{
    public partial class EventWatcher : Form
    {
        
        // Public methods
        public static bool IsRunning()
        {
            var fileLock = SimpleFileLock.Create(Constants.PIDLOCK_PATH);
            return (!fileLock.TestLockIsFree());
        }

        // Variables
        private SimpleFileLock fileLock = null;
        private RUN_MODE runMode;
        public enum RUN_MODE
        {
            MONITOR,
            ONCE
        }
        Action<int> debouncedWrapper = null;

        // Constructor
        public EventWatcher(RUN_MODE mode)
        {
            InitializeComponent();

            runMode = mode;

            Action<int> a = (arg) =>
            {
                DoWork(RUN_MODE.MONITOR);
            };

            debouncedWrapper = a.Debounce();

            
        }

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
                    OnMonitorStateChange();
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
                    OnMonitorStateChange();
                    break;
            }
        }

        #endregion

        #region "event handler"

        private void OnMonitorStateChange()
        {
            debouncedWrapper(3000); 
        }

        private void EventWatcher_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Constants.DEFAULT_ICON;

            // onLoad
            if (runMode == RUN_MODE.MONITOR)
            {
                // Set up pidfile to enforce a single instance of the monitor
                var fileLock = SimpleFileLock.Create(Constants.PIDLOCK_PATH);
                if (!fileLock.TryAcquireLock())
                {
                    Close();
                    // Another instance of this program is already running on the machine (silent exit)
                    return;
                }

                SystemEvents.PowerModeChanged += OnPowerChange;
                this.FormClosed += EventWatcher_FormClosed;

                // Run it now
                DoWork();
            }
            else if (runMode == RUN_MODE.ONCE)
            {
                // Run it now
                DoWork();
                Close();
            }
        }

        private void EventWatcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (fileLock != null)
            {
                fileLock.ReleaseLock();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoWork();
            }
        }

        #endregion

        #region "main logic"
        private delegate void DoWorkDelegate(RUN_MODE mode);
        void DoWork(RUN_MODE mode = RUN_MODE.ONCE)
        {
            if (InvokeRequired)
                Invoke(new DoWorkDelegate(DoWork), mode);
            else
            {
                string reason = (mode == RUN_MODE.MONITOR) ? "Display change detected" : "Applied settings";

                var res = Program.Run();
                if (res == ApplyResult.Success_WriteOK)
                {
                    FlashIcon(Constants.OK_ICON);
                    if (!Program.SilentMode) ShowBalloon($"{reason}\n\nPowerPoint will output on display \"{Config.LoadConfig().GetShortName()}\"");
                }
                else if (res == ApplyResult.Fail_NotDetected)
                {
                    FlashIcon(Constants.NO_ICON);
                    if (!Program.SilentMode) ShowBalloon($"Failed to apply settings\n\nDid not detect display \"{Config.LoadConfig().GetShortName()}\"");
                }
            }
        }

        #endregion

        #region "balloon ui"
        private void ShowBalloon(string title)
        {
            //title = (title.Length >= 64) ? title.Substring(0, 60) + "..." : title;
            notifyIcon1.BalloonTipText = title;
            notifyIcon1.ShowBalloonTip(30);
        }
        #endregion

        #region "notification tray icon"
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

        private void IconFlashTimer_Tick(object sender, EventArgs e)
        {
            flashCounter += 1;
            
            if (flashCounter > 6)
            {
                iconFlashTimer.Stop();
                notifyIcon1.Icon = Constants.DEFAULT_ICON;

                if (this.runMode == RUN_MODE.ONCE)
                {
                    Close();
                }

                return;
            }

            notifyIcon1.Icon = (flashCounter % 2 == 0) ? icon : Constants.DEFAULT_ICON;
        }
        #endregion
    }
}
