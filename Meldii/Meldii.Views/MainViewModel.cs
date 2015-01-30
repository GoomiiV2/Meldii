using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Common.WPF;
using Meldii.AddonProviders;

namespace Meldii.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public SortableObservableCollection<AddonMetaData> LocalAddons { get; set; }

        private string _StatusMessage;
        public string StatusMessage { get { return _StatusMessage; } set { _StatusMessage = value; NotifyPropertyChanged("StatusMessage"); } }

        public int SelectedAddonIndex = -1;
        public AddonMetaData _SelectedAddon;
        public AddonMetaData SelectedAddon { get { return _SelectedAddon; } set { _SelectedAddon = value; NotifyPropertyChanged("SelectedAddon"); } }
        public bool _IsPendingVersionCheck = true;

        public SettingsView _SettingsView;
        public SettingsView SettingsView { get { return _SettingsView; } set { _SettingsView = value; NotifyPropertyChanged("SettingsView"); } }

        public HelpView _HelpView;
        public HelpView HelpViewa { get { return _HelpView; } set { _HelpView = value; NotifyPropertyChanged("HelpViewa"); } }
        public MainViewModel()
        {
            HelpViewa = new HelpView();
            SettingsView = new SettingsView();

            StatusMessage = "Checking for new addon updates.....";
            LocalAddons = new SortableObservableCollection<AddonMetaData>();
        }


        public void SortAddonList()
        {
            App.Current.Dispatcher.BeginInvoke((Action)delegate()
            {
                LocalAddons.Sort(x => x.Name, ListSortDirection.Ascending);
            });
        }

        public bool IsPendingVersionCheck
        {
            get
            {
                return _IsPendingVersionCheck;
            }

            set
            {
                _IsPendingVersionCheck = value;
                NotifyPropertyChanged("IsPendingVersionCheck");
            }
        }

        public void OnOpenAddonPage()
        {
            if (SelectedAddon != null && SelectedAddon.AddonPage != null && SelectedAddon.AddonPage.Length > 0)
                Process.Start(SelectedAddon.AddonPage);
        }

        public void OnSelectedAddon(int SelectedIdx)
        {
            SelectedAddonIndex = SelectedIdx;
            if (SelectedIdx >= 0 && SelectedIdx < LocalAddons.Count)
            {
                SelectedAddon = LocalAddons[SelectedIdx];
            }
        }

        public void UpdateAddon()
        {
            if (SelectedAddonIndex >= 0 && SelectedAddonIndex < LocalAddons.Count)
            {
                AddonManager.Self.UpdateAddon(SelectedAddon);
            }
        }

        public async void AddonDeleteFromLibrary()
        {
            if (SelectedAddonIndex >= 0 && SelectedAddonIndex < LocalAddons.Count)
            {
                if (await MainWindow.ShowMessageDialogYesNo("Are you sure?", string.Format("This will delete {0} Version: {1} from your addon Library.", SelectedAddon.Name, SelectedAddon.Version)))
                {
                    AddonManager.Self.DeleteAddonFromLibrary(SelectedAddonIndex);
                }
            }
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
