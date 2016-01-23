using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
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

        public async void OnOpenAddonPage()
        {
            if (SelectedAddon != null && SelectedAddon.AddonPage != null && SelectedAddon.AddonPage.Length > 0)
                Process.Start(SelectedAddon.AddonPage);
            else
            {
                await MainWindow.ShowMessageDialogYesNo("Error opening addon page", "Either this addon has no addon page set or it was a malformed url format");
            }
        }

        public void OpenAddonLocation()
        {
             if (SelectedAddonIndex >= 0 && SelectedAddonIndex < LocalAddons.Count)
             {
                 string dest = SelectedAddon.IsAddon ? Statics.AddonsFolder : Statics.GetPathForMod(SelectedAddon.Destination);
                 Process.Start(dest);
             }
        }

        public void CheckForUpdates()
        {
            AddonManager.Self.CheckAddonsForUpdates();
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
