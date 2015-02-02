using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.AddonProviders;
using Meldii.DataStructures;

namespace Meldii.Views
{
    public class SettingsView : INotifyPropertyChanged
    {
        private string _FirefallInstall;
        private string _AddonLibaryPath;
        private bool _IsMelderProtcolEnabled;

        public SettingsView()
        {
            FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
            IsMelderProtcolEnabled = MeldiiSettings.Self.IsMelderProtcolEnabled;
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

        public void SaveSettings()
        {
            bool hasAddonLibFolderChanged = (MeldiiSettings.Self.AddonLibaryPath != _AddonLibaryPath);

            MeldiiSettings.Self.FirefallInstallPath = _FirefallInstall;
            MeldiiSettings.Self.AddonLibaryPath = _AddonLibaryPath;
            MeldiiSettings.Self.IsMelderProtcolEnabled = _IsMelderProtcolEnabled;
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
                catch (Exception e)
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
