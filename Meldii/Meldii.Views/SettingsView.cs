using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.DataStructures;

namespace Meldii.Views
{
    public class SettingsView : INotifyPropertyChanged
    {
        private string _FirefallInstall;
        private string _AddonLibaryPath;

        public SettingsView()
        {
            _FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            _AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
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
            get { return MeldiiSettings.Self.AddonLibaryPath; }

            set
            {
                _AddonLibaryPath = value;
                NotifyPropertyChanged("AddonLibaryPath");
            }
        }

        public void SaveSettings()
        {
            MeldiiSettings.Self.FirefallInstallPath = _FirefallInstall;
            MeldiiSettings.Self.AddonLibaryPath = _AddonLibaryPath;
            MeldiiSettings.Self.Save();
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
