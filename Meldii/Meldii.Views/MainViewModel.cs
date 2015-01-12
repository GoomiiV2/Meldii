using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Meldii.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AddonMetaData> LocalAddons { get; set; }

        private string _StatusMessage;
        public string StatusMessage { get { return _StatusMessage; } set { _StatusMessage = value; NotifyPropertyChanged("StatusMessage"); } }

        public AddonMetaData _SelectedAddon;
        public AddonMetaData SelectedAddon { get { return _SelectedAddon; } set { _SelectedAddon = value; NotifyPropertyChanged("SelectedAddon"); } }


        public MainViewModel()
        {
            StatusMessage = "Checking for new addon updates.....";
            LocalAddons = new ObservableCollection<AddonMetaData>();
        }

        public void OnOpenAddonPage()
        {
            if (SelectedAddon != null)
                Process.Start(SelectedAddon.AddonPage);
        }

        public void OnSelectedAddon(int SelectedIdx)
        {
            SelectedAddon = LocalAddons[SelectedIdx];
            StatusMessage = "OnSelectedAddon";
        }


        // Gah
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
