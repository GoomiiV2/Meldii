using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Meldii.AddonProviders;
using Meldii.DataStructures;
using Meldii.Views;
using Meldii.Windows;

namespace Meldii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Self = null;

        MainViewModel ViewModel = null;
        AddonManager AddonManager = null;
        public bool IsSettingsWindowOpen = false;
        public bool IsHelpWindowOpen = false;

        public MainWindow()
        {
            Self = this;

            InitializeComponent();
            Statics.InitStaticData();
            MeldiiSettings.Self.Load();
            ViewModel = new MainViewModel();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ViewModel;
            AddonManager = new AddonManager(ViewModel);
            Statics.AddonManager = AddonManager;

            SelfUpdater.ThreadUpdateAndCheck();
        }

        private void Btt_OpenAddonPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OnOpenAddonPage();
        }

        private void Btt_OpenSettings(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(0);
        }

        private void Btt_OpenHelp(object sender, RoutedEventArgs e)
        {
            this.ToggleFlyout(1);
        }

        private void AddonDownloadUpdate(object sender, MouseButtonEventArgs e)
        {
            ViewModel.UpdateAddon();
        }

        private void AddonDeleteFromLibrary(object sender, MouseButtonEventArgs e)
        {
            ViewModel.AddonDeleteFromLibrary();
        }

        public static async void UpdatePromt()
        {
            if (await MainWindow.ShowMessageDialogYesNo("Download Update?", "Melii update available"))
            {
                SelfUpdater.Update();
            }
        }

        public static async void ShowAlert(string title, string msg)
        {
            if (await MainWindow.ShowMessageBox(title, msg))
            {

            }
        }

        public static async Task<bool> ShowMessageDialogYesNo(string title, string message)
        {
            Task<bool> value = Self._ShowMessageDialogYesNo(title, message);
            return await value;
        }

        private async Task<bool> _ShowMessageDialogYesNo(string title, string message)
        {
            MetroDialogOptions.ColorScheme = true ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                ColorScheme = true ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.ShowMessageAsync(title, message,
            MessageDialogStyle.AffirmativeAndNegative, mySettings);
            return (result == MessageDialogResult.Affirmative);
        }

        public static async Task<bool> ShowMessageBox(string title, string message)
        {
            Task<bool> value = Self._ShowMessageBox(title, message);
            return await value;
        }

        private async Task<bool> _ShowMessageBox(string title, string message)
        {
            MetroDialogOptions.ColorScheme = true ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Ok",
                ColorScheme = true ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.ShowMessageAsync(title, message,
            MessageDialogStyle.Affirmative, mySettings);
            return (result == MessageDialogResult.Affirmative);
        }

        private void AddonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.OnSelectedAddon(AddonList.SelectedIndex);
        }

        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }
            flyout.IsOpen = !flyout.IsOpen;
        }
    }
}
