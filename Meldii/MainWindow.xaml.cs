using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Meldii.AddonProviders;
using Meldii.DataStructures;
using Meldii.Views;

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

            // Admin Check
            if (Statics.NeedAdmin())
                Statics.RunAsAdmin(Statics.LaunchArgs);

            ViewModel = new MainViewModel();

            this.AddHandler(MetroWindow.DragOverEvent, new DragEventHandler(OnFileDragOver), true);
            this.AddHandler(MetroWindow.DropEvent, new DragEventHandler(OnFileDrop), true);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ViewModel;
            AddonManager = new AddonManager(ViewModel);
            Statics.AddonManager = AddonManager;

            SelfUpdater.ThreadUpdateAndCheck();
        }

        private void Btt_LaunchFirefall(object sender, RoutedEventArgs e)
        {
            string[] paths = new string[] {
                MeldiiSettings.Self.FirefallInstallPath,
                "system",
                "bin"
            };
            string p = System.IO.Path.Combine(paths);

            // Our process information.
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = System.IO.Path.Combine(p, "FirefallClient.exe");
            info.WorkingDirectory = p;
            info.UseShellExecute = false;

            // Launch it now.
            try
            {
                Process.Start(info);
                //Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                ShowAlert("Launch error", "Error launching application.\nCould not find FirefallClient.exe.\n\nSearch location: " + p);
            }
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

        private void OnFileDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = false;
        }

        private async void OnFileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                var controller = await MainWindow.Self.ShowProgressAsync("Please wait...", "Copying addons to library");

                Thread t = new Thread(new ThreadStart(delegate
                {
                    foreach (string file in docPath)
                    {
                        try
                        {
                            if (System.IO.Path.GetExtension(file) == ".zip")
                                File.Copy(file, System.IO.Path.Combine(MeldiiSettings.Self.AddonLibaryPath, System.IO.Path.GetFileName(file)));
                        }
                        catch (Exception)
                        {

                        }
                    }

                    controller.CloseAsync();
                }));

                t.IsBackground = true;
                t.Start();
            }
        }

        private void Btt_OpenAddonLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (MeldiiSettings.Self.AddonLibaryPath != null || MeldiiSettings.Self.AddonLibaryPath != "" || MeldiiSettings.Self.AddonLibaryPath != " ")
            {
                Process.Start(MeldiiSettings.Self.AddonLibaryPath);
            }
        }

        private void Btt_OpenAddonIndex_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://forums.firefall.com/community/threads/addon-index-current-addons-mods-and-dev-resources.2625421/");
        }

        public static async void UpdatePromt()
        {
            if (await MainWindow.ShowMessageDialogYesNo("Meldii update available", "Download Update?"))
            {
                var controller = await MainWindow.Self.ShowProgressAsync("Please wait...", "Downloading Meldii update\nMeldii will restart when it is done");

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
