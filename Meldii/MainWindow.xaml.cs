using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro;
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

        public MainViewModel ViewModel = null;
        public AddonManager AddonManager = null;
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

            this.AddHandler(MetroWindow.DragOverEvent, new DragEventHandler(OnFileDragOver), true);
            this.AddHandler(MetroWindow.DropEvent, new DragEventHandler(OnFileDrop), true);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new MainViewModel();
            AddonManager = new AddonManager(ViewModel);

            DataContext = ViewModel;
            Statics.AddonManager = AddonManager;

            Statics.GetFirefallPatchData();

            if (Statics.ShouldUpdate)
                SelfUpdater.Update();
            else
                SelfUpdater.ThreadUpdateAndCheck();
        }

        private void Btt_LaunchFirefall(object sender, RoutedEventArgs e)
        {
            LaunchFirefallProcess("FirefallClient.exe");
        }

        public static void LaunchFirefallProcess(string process)
        {
            string[] paths = new string[] {
                MeldiiSettings.Self.FirefallInstallPath,
                "system",
                "bin"
            };
            string p = System.IO.Path.Combine(paths);

            // Our process information.
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = System.IO.Path.Combine(p, process);
            info.WorkingDirectory = p;
            info.UseShellExecute = false;

            // Launch it now.
            try
            {
                if (MeldiiSettings.Self.LaunchFirefallWithSteam)
                    Process.Start("steam://run/227700");
                else
                    Process.Start(info);

                if (MeldiiSettings.Self.CloseMeldiiOnFirefallLaunch)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate()
                    {
                        Application.Current.Shutdown();
                    });
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                ShowAlert("Launch error", "Error launching application.\nCould not find " + process + ".\n\nSearch location: " + p);
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

        private void Btt_OpenAddonLocation( object sender, MouseButtonEventArgs e)
        {
            ViewModel.OpenAddonLocation();
        }

        private void Btt_CheckForUpdates_Click( object sender, MouseButtonEventArgs e)
        {
            ViewModel.CheckForUpdates();
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

        public static async void UpdatePrompt()
        {
            if (await MainWindow.ShowMessageDialogYesNo("Meldii update available", "Download Update?"))
            {
                SelfUpdater.RestartForUpdate();
            }
        }

        public static async void ShowUpdateProgress()
        {
            await MainWindow.Self.ShowProgressAsync("Please wait...", "Downloading Meldii update\nMeldii will restart when it is done");
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
            var items = this.Flyouts.Items;

            for (int i = 0; i < items.Count; i++)
            {
                var flyout = items[i] as Flyout;

                if (i == index)
                {
                    flyout.IsOpen = !flyout.IsOpen;
                }
                else
                {
                    flyout.IsOpen = false;
                }
            }
        }

        public static void SetAppTheme(string Theme, string Accent)
        {
            var theme = ThemeManager.GetAppTheme(Theme);
            var accent = ThemeManager.GetAccent(Accent);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
