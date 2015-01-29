using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Meldii.Properties;

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
            Thread updateCheck = new Thread(() => 
            {
                // Check for updater and remove it if it is there
                if (File.Exists(Statics.UpdaterName))
                {
                    File.Delete(Statics.UpdaterName);
                }

                if (IsUpdateAvailable() || true)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        MainWindow.UpdatePromt();
                    });
                }
           });

            updateCheck.IsBackground = true;
            updateCheck.Start();
        }
    }
}
