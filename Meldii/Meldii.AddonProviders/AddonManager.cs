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
            GetMelderInstalledAddons(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, Statics.ModDataStoreReltivePath)); // Mods
        }

        // Genrate a list of addons that we have locally
        public void GetLocalAddons()
        {
            MainView.StatusMessage = "Discovering Addon libary";

            MainView.LocalAddons.Clear();

            string path = MeldiiSettings.Self.AddonLibaryPath;

            string[] fileEntries = Directory.GetFiles(path, "*.zip");
            foreach (string fileName in fileEntries)
            {
                AddonMetaData addon = ParseZipForIni(fileName);
                if (addon != null)
                {
                    addon.ZipName = fileName;
                    MainView.LocalAddons.Add(addon);
                }
            }
        }

        public void GetMelderInstalledAddons(string path)
        {
            MainView.StatusMessage = "Discovering Installed Addons...";

            string[] fileEntries = Directory.GetFiles(path, "*.ini");
            foreach (string fileName in fileEntries)
            {
                using (TextReader reader = File.OpenText(fileName))
                {
                    AddonMetaData addon = new AddonMetaData();
                    addon.ReadFromIni(reader);

                    var FullAddon = GetAddonLocalByNameAndVersion(addon.Name, addon.Version);
                    FullAddon.IsEnabled = true;
                    FullAddon.InstalledFilesList = addon.InstalledFilesList;
                }

            }
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

                MainView.StatusMessage = "All Addons have been checked for updates :3";
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

        // Installing and uninstalling of addon, could be neater >,>
        public void InstallAddon(AddonMetaData addon)
        {
            MainView.StatusMessage = string.Format("Installing Addon {0}", addon.Name);

            addon.InstalledFilesList.Clear();

            string dest = addon.IsAddon ? Statics.AddonsFolder : Statics.GetPathForMod(addon.Destnation);
            string installInfoDest = addon.IsAddon ? Path.Combine(Statics.AddonsFolder, "melder_addons") : Path.Combine(MeldiiSettings.Self.FirefallInstallPath, Statics.ModDataStoreReltivePath);
            installInfoDest = Path.Combine(installInfoDest, Path.GetFileName(addon.ZipName) + ".ini");

            // Prep for back up
            ZipFile BackupZip = null;
            if (!addon.IsAddon)
            {
                string backuppath = Statics.GetBackupPathForMod(Path.GetFileNameWithoutExtension(addon.ZipName));

                if (File.Exists(backuppath) && Statics.IsPathSafe(backuppath))
                    File.Delete(backuppath);

                BackupZip = new ZipFile(backuppath);
            }

            // We go over the files one by one so that we can ingore files as we need to
            using (ZipFile zip = ZipFile.Read(addon.ZipName))
            {
                foreach (ZipEntry file in zip)
                {
                    // If its a mod back up any files that need backing up
                    if (!addon.IsAddon)
                    {
                        string modFilePath = Path.Combine(dest, file.FileName);
                        if (File.Exists(modFilePath) && Statics.IsPathSafe(dest))
                        {
                            BackupZip.AddFile(modFilePath, addon.Destnation);
                        }
                    }

                    // Extract the files to their new home
                    // Make sure its not an ignored file
                    var hits = addon.IngoreFileList.Find(x => file.FileName.ToLower().Contains(x.ToLower()));
                    if (hits == null || hits.Length == 0 && Statics.IsPathSafe(dest))
                    {
                        file.Extract(dest, ExtractExistingFileAction.OverwriteSilently);

                        string installedPath = file.FileName;

                        if (addon.Destnation != null && !installedPath.Contains(addon.Destnation))
                            installedPath = Path.Combine(addon.Destnation, file.FileName);

                        addon.InstalledFilesList.Add(installedPath);

                    }
                }
            }

             if (!addon.IsAddon)
             {
                 BackupZip.Save();
             }

             addon.WriteToIni(installInfoDest);

            MainView.StatusMessage = string.Format("Addon {0} Installed", addon.Name);
        }

        public void UninstallAddon(AddonMetaData addon)
        {
            MainView.StatusMessage = string.Format("Uninstalling Addon {0}", addon.Name);

            // Addon, nice and easy
            if (addon.IsAddon)
            {
                // Remove all the installed files
                foreach (string filePath in addon.InstalledFilesList)
                {
                    string path = filePath;

                    if (path.StartsWith("Addons/")) // Melder added "Addons/" to the start when an addon was put under the my docs location so remove it
                        path = filePath.Replace("Addons/", "");

                    path = Statics.FixPathSlashes(Path.Combine(Statics.AddonsFolder, filePath));
                    if (File.Exists(path) && Statics.IsPathSafe(path))
                    {
                        File.Delete(path);
                    }
                    else if (Directory.Exists(path) && Statics.IsPathSafe(path))
                    {
                        Directory.Delete(path, true);
                    }
                }

                // The info File
                string infoPath = Path.Combine(Statics.AddonsFolder, "melder_addons", Path.GetFileName(addon.ZipName)+".ini");
                if (File.Exists(infoPath) && Statics.IsPathSafe(infoPath))
                {
                    File.Delete(infoPath);
                }
            }

            MainView.StatusMessage = string.Format("Addon {0} Uninstalled", addon.Name);
        }
    }
}
