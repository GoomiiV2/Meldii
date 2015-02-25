using System;
using System.ComponentModel;
using Meldii.AddonProviders;
using Meldii.DataStructures;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Meldii.Views
{
    public enum Themes
    {
        BaseLight,
        BaseDark
    }

    public enum ThemeAccents
    {
        Amber,
        Blue,
        Brown,
        Cobalt,
        Crimson,
        Cyan,
        Emerald,
        Green,
        Indigo,
        Lime,
        Magenta,
        Mauve,
        Olive,
        Orange,
        Pink,
        Purple,
        Red,
        Sienna,
        Steel,
        Taupe,
        Teal,
        Violet,
        Yellow
    }

    public class SettingsView : INotifyPropertyChanged
    {
        private string _FirefallInstall;
        private string _AddonLibaryPath;
        private bool _IsMelderProtocolEnabled;
        private bool _CloseMeldiiOnFirefallLaunch;
        private string _Theme;
        private string _ThemeAccent;
        private bool _CheckForPatchs;

        public SettingsView()
        {
            FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
            IsMelderProtocolEnabled = MeldiiSettings.Self.IsMelderProtocolEnabled;
            CloseMeldiiOnFirefallLaunch = MeldiiSettings.Self.CloseMeldiiOnFirefallLaunch;
            Theme = MeldiiSettings.Self.Theme != null ? MeldiiSettings.Self.Theme : "BaseDark";
            ThemeAccent = MeldiiSettings.Self.ThemeAccent != null ? MeldiiSettings.Self.ThemeAccent : "Purple";
            CheckForPatchs = MeldiiSettings.Self.CheckForPatchs;
        }

        //---UAC---------------------------------------------------------------
        #region DLL

        private const int MAX_PATH = 0x00000104;

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        private static extern Int32 SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern Int32 DestroyIcon(IntPtr hIcon);

        public BitmapSource GetShieldIcon()
        {
            BitmapSource shield = null;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                const uint SIID_SHIELD = 77;
                const uint SHGSI_ICON = 0x000000100;
                const uint SHGSI_SMALLICON = 0x000000001;

                SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
                sii.cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SIID_SHIELD, SHGSI_ICON | SHGSI_SMALLICON, ref sii));

                shield = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(sii.hIcon, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(sii.hIcon);
            }
            else
            {
                shield = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    System.Drawing.SystemIcons.Shield.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }

            return shield;
        }

        #endregion
        //---------------------------------------------------------------------

        // Ui binding hooks
        public string FirefallInstall
        {
            get { return _FirefallInstall; }

            set
            {
                _FirefallInstall = value;
                NotifyPropertyChanged("FirefallInstall");
            }
        }

        public string AddonLibaryPath
        {
            get { return _AddonLibaryPath; }

            set
            {
                _AddonLibaryPath = value;
                NotifyPropertyChanged("AddonLibaryPath");
            }
        }

        public bool IsMelderProtocolEnabled
        {
            get { return _IsMelderProtocolEnabled; }

            set
            {
                _IsMelderProtocolEnabled = value;
                NotifyPropertyChanged("IsMelderProtocolEnabled");

                // If we are changing our melder protocol, display the UAC shield.
                var flyout = MainWindow.Self.Flyouts.Items[0] as MahApps.Metro.Controls.Flyout;
                var settings = flyout.Content as Meldii.Windows.SettingsFlyout;
                if (_IsMelderProtocolEnabled != MeldiiSettings.Self.IsMelderProtocolEnabled)
                    settings.Img_UAC.Source = GetShieldIcon();
                else settings.Img_UAC.Source = null;
            }
        }

        public bool CloseMeldiiOnFirefallLaunch
        {
            get { return _CloseMeldiiOnFirefallLaunch; }

            set
            {
                _CloseMeldiiOnFirefallLaunch = value;
                NotifyPropertyChanged("CloseMeldiiOnFirefallLaunch");
            }
        }

        public string Theme
        {
            get { return _Theme; }

            set
            {
                _Theme = value;
                NotifyPropertyChanged("Theme");

                if (_ThemeAccent != null)
                    MainWindow.SetAppTheme(_Theme, _ThemeAccent);
            }
        }

        public string ThemeAccent
        {
            get { return _ThemeAccent; }

            set
            {
                _ThemeAccent = value;
                NotifyPropertyChanged("ThemeAccent");

                if (_Theme != null)
                    MainWindow.SetAppTheme(_Theme, _ThemeAccent);
            }
        }

        public bool CheckForPatchs
        {
            get { return _CheckForPatchs; }

            set
            {
                _CheckForPatchs = value;
                NotifyPropertyChanged("CheckForPatchs");
            }
        }

        public void SaveSettings()
        {
            bool hasAddonLibFolderChanged = (MeldiiSettings.Self.AddonLibaryPath != _AddonLibaryPath);
            bool hasMelderProtocolChanged = MeldiiSettings.Self.IsMelderProtocolEnabled != _IsMelderProtocolEnabled;

            MeldiiSettings.Self.FirefallInstallPath = _FirefallInstall;
            MeldiiSettings.Self.AddonLibaryPath = _AddonLibaryPath;
            MeldiiSettings.Self.IsMelderProtocolEnabled = _IsMelderProtocolEnabled;
            MeldiiSettings.Self.CloseMeldiiOnFirefallLaunch = _CloseMeldiiOnFirefallLaunch;
            MeldiiSettings.Self.Theme = Theme;
            MeldiiSettings.Self.ThemeAccent = ThemeAccent;
            MeldiiSettings.Self.CheckForPatchs = CheckForPatchs;
            MeldiiSettings.Self.Save();

            if (hasAddonLibFolderChanged)
            {
                try
                {
                    AddonManager.Self.GetLocalAddons();
                    AddonManager.Self.GetInstalledAddons();
                    AddonManager.Self.CheckAddonsForUpdates();
                    AddonManager.Self.SetupFolderWatchers();
                }
                catch (Exception)
                {

                }
            }

            if (hasMelderProtocolChanged)
            {
                if (MeldiiSettings.Self.IsMelderProtocolEnabled)
                    Statics.EnableMelderProtocol();
                else Statics.DisableMelderProtocol();

                IsMelderProtocolEnabled = IsMelderProtocolEnabled;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
