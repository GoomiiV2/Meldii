using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Meldii.DataStructures;
using Meldii.Views;

namespace Meldii.AddonProviders
{
    public class DirectDownload : ProviderBase
    {
        private static int maxRetrys = 3;
        private int retrys = 0;
        private string downloadsName = ""; // hack for one click downloads

        public DirectDownload()
        {
        }

        public bool DownloadFile(string url, string filePath, string addonName)
        {
            try
            {
                WebClient Dlii = new WebClient();
                Dlii.Headers["User-Agent"] = Properties.Settings.Default.Useragent;

                Dlii.DownloadFile(url, filePath);

                // Glorious hack for the motherland!
                string header = Dlii.ResponseHeaders["Content-Disposition"] ?? string.Empty;
                int index = header.LastIndexOf("filename=", StringComparison.OrdinalIgnoreCase);
                if (addonName == null && index > -1)
                {
                    addonName = header.Substring(index).Replace("filename=", "").Replace("\"", ""); ;
                    downloadsName = addonName;
                }
            }
            catch (WebException e)
            {
                // Most likey our session has expired so relogin
                if (e.Message == "The remote server returned an error: (403) Forbidden." && retrys < maxRetrys)
                {
                    retrys++;
                    return DownloadFile(url, filePath, addonName);
                }
                else
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        MainWindow.ShowAlert("Download Error", string.Format("There was an error when trying to download the file, please try again later.\n\nIf the error persists then the addon author more than likely has messed up the melder info in the thread so go bug them about it!\n\nAddon: {0}\nUrl: {1}", addonName, url));
                    });

                    return false;
                }
            }

            return true;
        }

        public override void Update(AddonMetaData addon)
        {
            string dest = Path.Combine(tempDlDir, addon.Name + ".zip");

            if (DownloadFile(addon.DownloadURL, dest, addon.Name))
                CopyUpdateToLibrary(addon.ZipName, dest);
        }

        public override void DownloadAddon(string url)
        {
            string dlPath = Path.Combine(tempDlDir, "oneClickDl.zip");

            // Lazy way to cope with alot of downloads at once
            if (File.Exists(dlPath))
                dlPath += new Random().Next(10);

            if (DownloadFile(url, dlPath, null))
            {
                string dest = Path.Combine(tempDlDir, downloadsName);
                CopyUpdateToLibrary(Path.Combine(MeldiiSettings.Self.AddonLibaryPath, downloadsName), dlPath);
            }
        }
    }
}
