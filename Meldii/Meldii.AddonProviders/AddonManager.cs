using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ionic.Zip;
using Meldii.DataStructures;
using Meldii.Views;
using Microsoft.Win32;

namespace Meldii.AddonProviders
{
    public enum AddonProviderType
    {
        FirefallFourms,
        GitHub,
        BitBucket,
        DirectDownload
    }

    public class AddonManager
    {
        private MainViewModel MainView = null;
        private Dictionary<AddonProviderType, ProviderBase> Providers = new Dictionary<AddonProviderType, ProviderBase>();

        public AddonManager(MainViewModel _MainView)
        {
            MainView = _MainView;
            Providers.Add(AddonProviderType.FirefallFourms, new FirefallFourms());

            if (Statics.IsFirstRun)
            {
                MeldiiSettings.Self.FirefallInstallPath = GetFirefallInstallPath();
                MeldiiSettings.Self.AddonLibaryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Properties.Settings.Default.AddonStorage;
                MeldiiSettings.Self.Save();

                // Migrate some melder data?
            }

            GetLocalAddons();
            CheckAddonsForUpdates();
            GetMelderInstalledAddons(Path.Combine(Statics.AddonsFolder, "melder_addons")); // Addons
            GetMelderInstalledAddons(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, "system\\melder_addons")); // Mods
        }

        // Genrate a list of addons that we have locally
        public void GetLocalAddons()
        {
            MainView.LocalAddons.Clear();

            string path = MeldiiSettings.Self.AddonLibaryPath;

            string[] fileEntries = Directory.GetFiles(path, "*.zip");
            foreach (string fileName in fileEntries)
            {
                AddonMetaData addon = ParseZipForIni(fileName);
                if (addon != null)
                {
                    MainView.LocalAddons.Add(addon);
                }
            }
        }

        public void GetMelderInstalledAddons(string path)
        {
            string[] fileEntries = Directory.GetFiles(path, "*.ini");
            foreach (string fileName in fileEntries)
            {
                using (TextReader reader = File.OpenText(fileName))
                {
                    AddonMetaData addon = new AddonMetaData();
                    addon.ReadFromIni(reader);
                    GetAddonLocalByNameAndVersion(addon.Name, addon.Version).IsEnabled = true;
                }

            }
        }

        public void CheckIfAddonIsInstalled(AddonMetaData addon)
        {

        }

        public bool CheckIfAddonIsMod(AddonMetaData AddonData)
        {
            return false;
        }

        private AddonMetaData ParseZipForIni(string path)
        {
            using (ZipFile zip = ZipFile.Read(path))
            {
                foreach (ZipEntry file in zip)
                {
                    if (file.FileName.ToUpper() == Properties.Settings.Default.MelderInfoName.ToUpper())
                    {
                        TextReader reader = new StreamReader(file.OpenReader());

                        AddonMetaData addon = new AddonMetaData();
                        addon.ReadFromIni(reader);
                        return addon;
                    }
                }
            }
            return null;

        }

        public void CheckAddonsForUpdates()
        {
            MainView.StatusMessage = "Checking Addons for updates......";

            Thread t = new Thread(new ThreadStart(delegate
            {
                Parallel.ForEach(MainView.LocalAddons, new ParallelOptions { MaxDegreeOfParallelism = 8 },
                    addon =>
                    {
                        var info = Providers[addon.ProviderType].GetMelderInfo(addon.AddonPage);
                        if (info != null)
                        {
                            addon.AvailableVersion = info.Version;
                            addon.IsUptoDate = IsAddonUptoDate(addon, info);
                            Debug.WriteLine("Addon: {0}, Version: {1}, Patch: {2}, Dlurl: {3}, IsUptodate: {4}", addon.Name, info.Version, info.Patch, info.Dlurl, addon.IsUptoDate);
                        }
                        else
                            addon.AvailableVersion = "??";
                    }
                );

                MainView.StatusMessage = "All Addons have been checked :3";
            }));
            t.IsBackground = true;
            t.Start();
        }

        public bool IsAddonUptoDate(AddonMetaData addon, MelderInfo melderInfo)
        {
            Version current = new Version(addon.Version);
            Version newVer = new Version(melderInfo.Version);

            return current.CompareTo(newVer) == 0 || current.CompareTo(newVer) == 1;
        }

        public AddonMetaData GetAddonLocalByNameAndVersion(string name, string version)
        {
            AddonMetaData addon = null;

            addon = MainView.LocalAddons.Single(x => x.Name == name && x.Version == version);

            return addon;
        }

        private string GetFirefallInstallPath()
        {
            return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Red 5 Studios\\Firefall_Beta", "InstallLocation", null);
        }
    }
}
