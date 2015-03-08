using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ionic.Zip;
using Meldii.DataStructures;
using Meldii.Views;

namespace Meldii.AddonProviders
{
    public enum AddonProviderType
    {
        [Description("Firefall Forums Attachment")]
        FirefallForums,
        /*,[Description("A Github Repo")]
        GitHub,
        [Description("A Bitbucket Repo")]
        BitBucket,*/
        [Description("Direct Download")]
        DirectDownload
    }

    public class AddonManager
    {
        public static AddonManager Self = null;
        private MainViewModel MainView = null;
        private Dictionary<AddonProviderType, ProviderBase> Providers = new Dictionary<AddonProviderType, ProviderBase>();
        private System.Object AddonsToUpdateLock = new System.Object();
        private List<AddonMetaData> AddonsToUpdate = new List<AddonMetaData>();
        private List<string> AddonsToRenableAfterUpdate = new List<string>();
        private FileSystemWatcher AddonLibaryWatcher = null;
        private int AddonsToBeUpdatedCount = 0;

        public AddonManager(MainViewModel _MainView)
        {
            Self = this;

            MainView = _MainView;
            Providers.Add(AddonProviderType.FirefallForums, new FirefallForums());
            Providers.Add(AddonProviderType.DirectDownload, new DirectDownload());

            GetLocalAddons();
            CheckAddonsForUpdates();
            GetInstalledAddons();
            SetupFolderWatchers();

            if (Statics.OneClickAddonToInstall != null)
            {
                Providers[Statics.OneClickInstallProvider].DownloadAddon(Statics.OneClickAddonToInstall);
                App.Current.Shutdown();
            }
        }

        public void GetInstalledAddons()
        {
            GetMelderInstalledAddons(Path.Combine(Statics.AddonsFolder, "melder_addons")); // Addons
            GetMelderInstalledAddons(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, Statics.ModDataStoreReltivePath)); // Mods
        }

        // Genrate a list of addons that we have locally
        public void GetLocalAddons()
        {
            MainView.StatusMessage = "Discovering Addon libary";

            MainView.LocalAddons.Clear();

            string path = MeldiiSettings.Self.AddonLibaryPath;

            if (Directory.Exists(path))
            {
                string[] fileEntries = Directory.GetFiles(path, "*.zip");
                foreach (string fileName in fileEntries)
                {
                    AddonMetaData addon = ParseZipForIni(fileName);
                    if (addon != null)
                    {
                        addon.ZipName = fileName;
                        addon.CheckIfAddonOrMod();
                        MainView.LocalAddons.Add(addon);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        public void GetMelderInstalledAddons(string path)
        {
            MainView.StatusMessage = "Discovering Installed Addons...";

            if (Directory.Exists(path))
            {
                string[] fileEntries = Directory.GetFiles(path, "*.ini");
                foreach (string fileName in fileEntries)
                {
                    using (TextReader reader = File.OpenText(fileName))
                    {
                        AddonMetaData addon = new AddonMetaData();
                        addon.ReadFromIni(reader);

                        var FullAddon = GetAddonLocalByNameAndVersion(addon.Name, addon.Version);

                        if (FullAddon != null)
                        {
                            FullAddon.IsEnabled = true;
                            FullAddon.InstalledFilesList = addon.InstalledFilesList;
                        }
                    }

                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        private AddonMetaData ParseZipForIni(string path)
        {
            using (ZipFile zip = ZipFile.Read(path))
            {
                foreach (ZipEntry file in zip)
                {
                    if (file.FileName.ToUpper().Contains(Properties.Settings.Default.MelderInfoName.ToUpper()))
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

        private void GetAddonUpdateInfo(AddonMetaData addon)
        {
            var info = Providers[addon.ProviderType].GetMelderInfo(addon.UpdateURL);
            if (addon != null && !addon.IsPendingDelete)
            {
                if (info != null)
                {
                    addon.AvailableVersion = info.Version;
                    addon.IsUptoDate = IsAddonUptoDate(addon, info);
                    addon.IsNotSuported = info.IsNotSuported;
                    addon.DownloadURL = info.Dlurl;

                    Debug.WriteLine("Addon: {0}, Version: {1}, Patch: {2}, Dlurl: {3}, IsUptodate: {4}", addon.Name, info.Version, info.Patch, info.Dlurl, addon.IsUptoDate);

                    if (!addon.IsUptoDate)
                    {
                        if (!addon._IsNotSuported)
                            AddonsToBeUpdatedCount++;

                        MainView.StatusMessage = string.Format("{0} Addons need to be updated", AddonsToBeUpdatedCount);
                    }
                }
                else
                    addon.AvailableVersion = "??";
            }
        }

        public void CheckAddonsForUpdates()
        {
            MainView.StatusMessage = "Checking Addons for updates......";

            try
            {
                Thread t = new Thread(new ThreadStart(delegate
                {
                    var exceptions = new ConcurrentQueue<Exception>();

                    MainView.IsPendingVersionCheck = true;

                    var addons = MainView.LocalAddons.ToArray();

                    Parallel.ForEach(addons, new ParallelOptions { MaxDegreeOfParallelism = 8 },
                        addon =>
                        {
                            try
                            {
                                if (addon != null && !addon.IsPendingDelete)
                                {
                                    GetAddonUpdateInfo(addon);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    );

                    MainView.IsPendingVersionCheck = false;

                    MainView.StatusMessage = string.Format("All Addons have been checked for updates, {0} can be updated", AddonsToBeUpdatedCount);
                }));

                t.IsBackground = true;
                t.Start();
            }
            catch (Exception)
            {

            }
        }

        public bool IsAddonUptoDate(AddonMetaData addon, MelderInfo melderInfo)
        {
            // There are many ways to make a bad version string :<
            addon.Version = Statics.CleanVersionString(addon.Version);
            melderInfo.Version = Statics.CleanVersionString(melderInfo.Version);


            try
            {
                Version current = new Version(addon.Version);
                Version newVer = new Version(melderInfo.Version);

                return current.CompareTo(newVer) == 0 || current.CompareTo(newVer) == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public AddonMetaData GetAddonLocalByNameAndVersion(string name, string version)
        {
            AddonMetaData addon = null;

            if (MainView.LocalAddons != null && MainView.LocalAddons.Count != 0)
            {
                addon = MainView.LocalAddons.FirstOrDefault(x => x.Name == name && x.Version == version);
            }

            return addon;
        }

        // Installing and uninstalling of addon, could be neater >,>
        // Refactor plz
        public void InstallAddon(AddonMetaData addon)
        {
            // Check we have a path for the Firefall install
            if (!Statics.IsFirefallInstallValid(MeldiiSettings.Self.FirefallInstallPath))
            {
                MainWindow.ShowAlert("Error: Invalid path to Firefall install!",
                    string.Format("\"{0}\" is not a valid Firefall install\nPlease go to settings and set the path to your Firefall install",
                    MeldiiSettings.Self.FirefallInstallPath));

                return;
            }

            MainView.StatusMessage = string.Format("Installing Addon {0}", addon.Name);

            addon.InstalledFilesList.Clear();

            string dest = addon.IsAddon ? Statics.AddonsFolder : Statics.GetPathForMod(addon.Destination);
            string installInfoDest = addon.IsAddon ? Path.Combine(Statics.AddonsFolder, "melder_addons") : Path.Combine(MeldiiSettings.Self.FirefallInstallPath, Statics.ModDataStoreReltivePath);
            installInfoDest = Path.Combine(installInfoDest, Path.GetFileName(addon.ZipName) + ".ini");

            // Prep for back up
            ZipFile BackupZip = null;
            if (!addon.IsAddon)
            {
                BackupZip = new ZipFile();

                // Back up files
                foreach (string file in addon.RemoveFilesList)
                {
                    string modFilePath = Path.Combine(dest, file.ToLower().Replace(addon.Destination.ToLower(), ""));
                    if (File.Exists(modFilePath) && Statics.IsPathSafe(modFilePath))
                    {
                        Debug.WriteLine("Install, backing up file: " + modFilePath);
                        string basePath = Path.GetDirectoryName(modFilePath.Replace(Statics.GetFirefallSystemDir(), ""));
                        BackupZip.AddFile(modFilePath, basePath);
                    }
                }

                using (ZipFile zip = ZipFile.Read(addon.ZipName))
                {
                    foreach (string file in zip.EntryFileNames)
                    {
                        string modFilePath = Path.Combine(dest, file);
                        if (File.Exists(modFilePath) && Statics.IsPathSafe(modFilePath))
                        {
                            Debug.WriteLine("Install, backing up file: " + modFilePath);
                            string basePath = Path.GetDirectoryName(modFilePath.Replace(Statics.GetFirefallSystemDir(), ""));
                            BackupZip.AddFile(modFilePath, basePath);
                        }
                    }
                }

                string backuppath = Statics.GetBackupPathForMod(Path.GetFileNameWithoutExtension(addon.ZipName));

                if (File.Exists(backuppath) && Statics.IsPathSafe(backuppath))
                    File.Delete(backuppath);

                BackupZip.Save(backuppath);
                BackupZip.Dispose();

                foreach (string file in addon.RemoveFilesList)
                {
                    string modFilePath = Path.Combine(dest, file.ToLower().Replace(addon.Destination.ToLower(), ""));
                    if (File.Exists(modFilePath) && Statics.IsPathSafe(modFilePath))
                    {
                        Debug.WriteLine("Install, removing file: " + modFilePath);
                        File.Delete(modFilePath);
                    }
                }
            }

            // We go over the files one by one so that we can ingore files as we need to
            using (ZipFile zip = ZipFile.Read(addon.ZipName))
            {
                foreach (ZipEntry file in zip)
                {
                    // Extract the files to their new home
                    // Make sure its not an ignored file
                    var hits = addon.IngoreFileList.Find(x => file.FileName.ToLower().Contains(x.ToLower()));
                    if (hits == null || hits.Length == 0 && Statics.IsPathSafe(dest))
                    {
                        file.Extract(dest, ExtractExistingFileAction.OverwriteSilently);

                        string installedPath = file.FileName;

                        if (addon.Destination != null && !installedPath.Contains(addon.Destination))
                            installedPath = Path.Combine(addon.Destination, file.FileName);

                        addon.InstalledFilesList.Add(installedPath);
                    }
                }
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
                    else if (Directory.Exists(path) && Statics.IsPathSafe(path) && Directory.GetFiles(path).Length == 0)
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
            else // Mods, remove and then restore backups
            {
                foreach (string filePath in addon.InstalledFilesList)
                {
                    string modFilePath = Statics.FixPathSlashes(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, "system", filePath));
                    if (File.Exists(modFilePath) && Statics.IsPathSafe(modFilePath))
                    {
                        Debug.WriteLine("Uninstall, Deleting file: " + modFilePath);
                        File.Delete(modFilePath);
                    }
                    else if (Directory.Exists(modFilePath) && Statics.IsPathSafe(modFilePath) && Directory.GetFiles(modFilePath).Length == 0) // Make sure it is empty
                    {

                    }
                }

                // Restore the backup
                string backUpFilePath = Statics.GetBackupPathForMod(Path.GetFileNameWithoutExtension(addon.ZipName));

                if (File.Exists(backUpFilePath))
                {
                    ZipFile backUp = new ZipFile(backUpFilePath);
                    backUp.ExtractAll(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, "system"), ExtractExistingFileAction.DoNotOverwrite);
                    backUp.Dispose();
                }

                // The info File
                string infoPath = Path.Combine(Path.Combine(MeldiiSettings.Self.FirefallInstallPath, Statics.ModDataStoreReltivePath), Path.GetFileName(addon.ZipName) + ".ini");
                if (File.Exists(infoPath) && Statics.IsPathSafe(infoPath))
                {
                    File.Delete(infoPath);
                }
            }

            MainView.StatusMessage = string.Format("Addon {0} Uninstalled", addon.Name);
        }

        // Add it to the list
        public void UpdateAddon(AddonMetaData addon)
        {
            lock (AddonsToUpdateLock)
            {
                AddonsToUpdate.Add(addon);
            }

            DoAddonUpdates();
        }

        public void DoAddonUpdates()
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                lock (AddonsToUpdateLock)
                {
                    foreach (AddonMetaData addon in AddonsToUpdate)
                    {
                        _UpdateAddon(addon);
                    }

                    AddonsToUpdate.Clear();
                }
            }));

            t.IsBackground = true;
            t.Start();
        }

        // Do the real updating
        public bool _UpdateAddon(AddonMetaData addon)
        {
            try
            {
                if (addon.DownloadURL != null && new Version(addon.Version) < new Version(addon.AvailableVersion))
                {
                    bool isInstalled = addon.IsEnabled;
                    if (isInstalled)
                    {
                        addon.IsEnabled = false; // The observable takes care of the actions
                        AddonsToRenableAfterUpdate.Add(addon.Name);
                    }

                    addon.IsUpdating = true;
                    Providers[addon.ProviderType].Update(addon);
                    addon.IsUpdating = false;

                    //if (isInstalled)
                    //addon.IsEnabled = isInstalled;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message);

                App.Current.Dispatcher.Invoke((Action)delegate 
                {
                    MainWindow.ShowAlert("Addon Update Error", "There was an error updating this addon "+ addon.Name);
                });

                return false;
            }
        }

        public async void DeleteAddonFromLibrary(int SelectedAddonIndex)
        {
            AddonMetaData addon = MainView.LocalAddons[SelectedAddonIndex];

            if (addon != null)
            {
                if (addon.IsEnabled)
                {
                    bool result = await MainWindow.ShowMessageDialogYesNo("Do you want to uninstall this addon?", string.Format("The addon has been removed from the library but is still installed\n Select yes to uninstall the addon or no to keep it installed", addon.Name));
                    
                    if (result)
                        UninstallAddon(addon);
                }

                if (File.Exists(addon.ZipName))
                {
                    MainView.StatusMessage = string.Format("Removed {0} Ver. {1} from the addon library", addon.Name, addon.Version);
                    File.Delete(addon.ZipName);
                    // Let the file watcher take care of updating the list?
                }
            }
        }

        // File watching for the addon lib folder, auto magicly update the list
        public void SetupFolderWatchers()
        {
            if (MeldiiSettings.Self.AddonLibaryPath != null && MeldiiSettings.Self.AddonLibaryPath != "" && MeldiiSettings.Self.AddonLibaryPath != " ")
            {
                if (AddonLibaryWatcher != null)
                {
                    AddonLibaryWatcher = null;
                }

                AddonLibaryWatcher = new FileSystemWatcher();
                AddonLibaryWatcher.Path = MeldiiSettings.Self.AddonLibaryPath;
                AddonLibaryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                AddonLibaryWatcher.Filter = "*.zip";
                AddonLibaryWatcher.Created += new FileSystemEventHandler(WatcherAdd);
                AddonLibaryWatcher.Deleted += new FileSystemEventHandler(WatcherRemove);

                AddonLibaryWatcher.EnableRaisingEvents = true;
            }
        }

        private void WatcherAdd(object source, FileSystemEventArgs e)
        {
            // Yo block untill its not in use! 
            while (Statics.IsFileLocked(new FileInfo(e.FullPath)))
            {
                Thread.Sleep(500);
            }

            AddonMetaData addon = ParseZipForIni(e.FullPath);
            if (addon != null)
            {
                addon.ZipName = e.FullPath;
                addon.CheckIfAddonOrMod(); // Important or else it will think it is a mod and will install to Firefall/system

                App.Current.Dispatcher.Invoke((Action)delegate 
                {
                    MainView.LocalAddons.Add(addon);
                });

                // Should we reenable this addon?
                if (AddonsToRenableAfterUpdate.Contains(addon.Name))
                {
                    addon.IsEnabled = true;
                    AddonsToRenableAfterUpdate.Remove(addon.Name);
                }

                MainView.SortAddonList();
                GetAddonUpdateInfo(addon);
            }
        }

        private void WatcherRemove(object source, FileSystemEventArgs e)
        {
            AddonMetaData addon = null;

            if (MainView.LocalAddons != null && MainView.LocalAddons.Count != 0)
            {
                addon = MainView.LocalAddons.FirstOrDefault(x => x.ZipName == e.FullPath);

                if (addon != null)
                {
                    addon.IsPendingDelete = true;

                    // wpf oh you be crazy sometime with execptions
                    App.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        if (MainView.LocalAddons.Contains(addon))
                        {
                            int idx = MainView.LocalAddons.IndexOf(addon);
                            if (idx != -1 && idx < MainView.LocalAddons.Count)
                            {
                                addon = null;
                                addon = MainView.LocalAddons[idx];
                                addon = null;
                                MainView.LocalAddons.RemoveAt(idx);
                            }
                        }
                    });
                }
            }
        }
    }
}
