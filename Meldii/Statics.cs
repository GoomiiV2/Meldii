using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Meldii.AddonProviders;
using Meldii.DataStructures;
using Microsoft.Win32;

namespace Meldii
{
    public class Statics
    {
        public static string UpdateCheckUrl = "https://raw.githubusercontent.com/GoomiChan/Meldii/master/Release/version.txt";
        public static string UpdateExeUrl = "https://raw.githubusercontent.com/GoomiChan/Meldii/master/Release/Meldii.exe";
        public static string UpdaterName = "Meldii.Updater.exe";
        public static string LaunchArgs = "";
        public static FirefallPatchData FirefallPatchData = null;

        public static string MeldiiAppData = "";
        public static string SettingsPath = "";
        public static string AddonsFolder = "";
        public static bool IsFirstRun = true;
        public static string DefaultAddonLocation = "gui\\components\\MainUI\\Addons";
        public static string AddonBackupPostfix = ".zip_backup.zip";
        public static string ModDataStoreReltivePath = "system\\melder_addons";
        public static string[] BlackListedPaths = 
        {
            "\\bin\\",
            "\\gui\\UISets\\",
            "\\gui\\components\\LoginUI\\"
        };
        public static string MelderProtcolRegex = "melder://(.*?)/(.*?):(.*)";

        public static AddonProviderType OneClickInstallProvider;
        public static string OneClickAddonToInstall = null; // the url of a forum attachment to install

        public static AddonManager AddonManager = null;

        public static void InitStaticData()
        {
            MeldiiAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Meldii");
            SettingsPath = Path.Combine(MeldiiAppData, "settings.json");

            if (!Directory.Exists(MeldiiAppData))
            {
                Directory.CreateDirectory(MeldiiAppData);
            }

            IsFirstRun = !File.Exists(SettingsPath);

            if (Statics.IsFirstRun)
            {
                MeldiiSettings.Self.FirefallInstallPath = GetFirefallInstallPath();
                MeldiiSettings.Self.AddonLibaryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Properties.Settings.Default.AddonStorage;
                MeldiiSettings.Self.Save();
            }

            AddonsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Firefall\Addons");
        }

        public static string FixPathSlashes(string str)
        {
            return str.Replace("/", "\\");
        }

        // Get the base path to install the addon at
        public static string GetPathForMod(string addonDest)
        {
            return Path.Combine(new string[] 
            {
                MeldiiSettings.Self.FirefallInstallPath,
                "system",
                addonDest
            });
        }

        public static bool IsFirefallInstallValid(string path)
        {
            string fp = Path.Combine(path, "system", "bin", "FirefallClient.exe");
            return File.Exists(fp);

        }

        public static bool IsFirefallInstallSteam()
        {
            var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var firefall = view.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 227700")) // Firefall app id
            {
                return firefall != null;
            }
        }

        // Get the path to the addons files backup
        public static string GetBackupPathForMod(string addonName)
        {
            return Path.Combine(new string[] 
            {
                MeldiiSettings.Self.FirefallInstallPath,
                ModDataStoreReltivePath,
                addonName
            }) + AddonBackupPostfix;
        }

        public static bool IsPathSafe(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (fullPath.Contains(AddonsFolder) || fullPath.Contains(MeldiiSettings.Self.FirefallInstallPath)) // Only allow under the addons location or the game install
            {
                string relPath = fullPath.Replace(AddonsFolder, "");
                relPath = relPath.Replace(MeldiiSettings.Self.FirefallInstallPath, "");

                foreach (string badii in BlackListedPaths)
                {
                    if (relPath.StartsWith(AddonsFolder))
                    {
                        Debug.WriteLine("IsPathSafe check failed on: {0}", relPath);
                        return false;
                    }
                }

                return true;
            }
            else
            {
                Debug.WriteLine("IsPathSafe check failed on: {0} is outside the game install or the addons folder", fullPath);
                return false;
            }
        }

        // http://stackoverflow.com/a/5238116
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        public static void EnableMelderProtocol()
        {
            if (Registry.ClassesRoot.OpenSubKey("melder") == null || (string)Registry.ClassesRoot.OpenSubKey("melder").OpenSubKey("shell").OpenSubKey("open").OpenSubKey("command").GetValue("") != "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"")
            {
                try
                {
                    RegistryKey key = Registry.ClassesRoot.CreateSubKey("melder");
                    key.SetValue("", "URL:Melder Protocol");
                    key.SetValue("URL Protocol", "");
                    key.CreateSubKey("DefaultIcon").SetValue("", "Meldii.exe,1");
                    key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");
                    key.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        public static void DisableMelderProtocol()
        {
            if (Registry.ClassesRoot.OpenSubKey("melder") != null)
            {
                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree("melder");
                }
                catch (Exception)
                {

                }
            }
        }

        public static FirefallPatchData GetFirefallPatchData()
        {
            if (FirefallPatchData == null)
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        using (Stream s = GenerateStreamFromString(wc.DownloadString("http://operator.firefallthegame.com/api/v1/products/Firefall_Beta")))
                        {
                            FirefallPatchData = FirefallPatchData.Create(s);
                        }
                    }
                    catch (System.Net.WebException)
                    {
                        FirefallPatchData = FirefallPatchData.CreateError();
                    }
                }
            }

            return FirefallPatchData;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetFirefallInstallPath()
        {
            string ffpath = String.Empty;

            var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var firefall = view.OpenSubKey(@"Software\Red 5 Studios\Firefall_Beta"))
            {
                // Get the install location and unbox.
                var loc = firefall.GetValue("InstallLocation");
                if (loc != null) ffpath = (string)loc;
            }

            return ffpath;
        }

        public static bool NeedAdmin()
        {
            try
            {
                if (IsFirefallInstallValid(MeldiiSettings.Self.FirefallInstallPath))
                {
                    string path = Path.Combine(MeldiiSettings.Self.FirefallInstallPath, "Meldii Admin Check.test");
                    File.WriteAllText(path, "Medlii admin check");
                    File.Delete(path);
                    return false;
                }
            }
            catch (Exception)
            {
                return true;
            }
            return false;
        }

        public static void RunAsAdmin(string args)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("Meldii.exe", args);
                info.Verb = "runas";
                Process.Start(info);

                App.Current.Dispatcher.Invoke((Action)delegate()
                {
                    Application.Current.Shutdown();
                });
            }
            catch { }
        }
    }
}
