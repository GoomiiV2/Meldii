using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meldii.DataStructures;

namespace Meldii.Views
{
    public class SettingsView : INotifyPropertyChanged
    {

        // Ui binding hooks
        public string FirefallInstall
        {
            get { return MeldiiSettings.Self.FirefallInstallPath; }

            set
            {

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
