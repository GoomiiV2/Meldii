using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Meldii
{
    public static class SelfUpdater
    {
        // Check for an update
        public static bool IsUpdateAvailable()
        {
            WebClient client = new WebClient();
            string PageData = client.DownloadString(Statics.UpdateCheckUrl);

            Version webVersion = new Version(PageData);

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version LocalVersion = new Version(fvi.FileVersion);

            return (webVersion > LocalVersion);
        }

        // Start the updater
        public static void Update()
        {
            Thread Update = new Thread(() =>
            {
                Stream output = File.OpenWrite(Statics.UpdaterName);
                output.Write(Meldii.Properties.Resources.Meldii_Updater, 0, Meldii.Properties.Resources.Meldii_Updater.Length);
                output.Flush();
                output.Close();

                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile(Statics.UpdateExeUrl, "Meldii_New.exe");
                }

                Process.Start(Statics.UpdaterName);

                App.Current.Dispatcher.Invoke((Action)delegate()
                {
                    Application.Current.Shutdown();
                });
            });

            Update.IsBackground = true;
            Update.Start();
        }

        public static void ThreadUpdateAndCheck()
        {
            Task t = new Task(UpdateChecks);
            t.Start();
        }

        private static async void UpdateChecks()
        {
            // If we aren't using a Steam install, check for a Firefall update.
            if (!String.IsNullOrEmpty(Statics.GetFirefallInstallPath()))
                await FirefallUpdate();

            // Check for updater and remove it if it is there
            if (File.Exists(Statics.UpdaterName))
            {
                File.Delete(Statics.UpdaterName);
            }

            if (IsUpdateAvailable())
            {
                MainWindow.UpdatePrompt();
            }
        }

        private static async Task<bool> FirefallUpdate()
        {
            // Check for a new Firefall patch.
            if (!Statics.FirefallPatchData.error)
            {
                var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using (var firefall = view.OpenSubKey(@"Software\Red 5 Studios\Firefall_Beta"))
                {
                    var version = firefall.GetValue("InstalledVersion");
                    if (version != null && (string)version != Statics.FirefallPatchData.build)
                    {
                        if (await MainWindow.ShowMessageDialogYesNo("Firefall update available", "Start the Launcher to download the update?"))
                        {
                            MainWindow.LaunchFirefallProcess("Launcher.exe");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
