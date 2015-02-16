using System;
using System.ComponentModel;
using Meldii.AddonProviders;
using Meldii.DataStructures;

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
        private bool _IsMelderProtcolEnabled;
        private string _Theme;
        private string _ThemeAccent;

        public SettingsView()
        {
            FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
            IsMelderProtcolEnabled = MeldiiSettings.Self.IsMelderProtcolEnabled;
            Theme = MeldiiSettings.Self.Theme != null ? MeldiiSettings.Self.Theme : "BaseDark";
            ThemeAccent = MeldiiSettings.Self.ThemeAccent != null ? MeldiiSettings.Self.ThemeAccent : "Purple";
        }

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

        public bool IsMelderProtcolEnabled
        {
            get { return _IsMelderProtcolEnabled; }

            set
            {
                _IsMelderProtcolEnabled = value;
                NotifyPropertyChanged("AddonLibaryPath");
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

        public void SaveSettings()
        {
            bool hasAddonLibFolderChanged = (MeldiiSettings.Self.AddonLibaryPath != _AddonLibaryPath);

            MeldiiSettings.Self.FirefallInstallPath = _FirefallInstall;
            MeldiiSettings.Self.AddonLibaryPath = _AddonLibaryPath;
            MeldiiSettings.Self.IsMelderProtcolEnabled = _IsMelderProtcolEnabled;
            MeldiiSettings.Self.Theme = Theme;
            MeldiiSettings.Self.ThemeAccent = ThemeAccent;
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

            if (MeldiiSettings.Self.IsMelderProtcolEnabled)
            {
                Statics.EnableMelderProtocol();
            }
            else
            {
                Statics.DisableMelderProtocol();
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
