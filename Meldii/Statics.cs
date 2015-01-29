using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        // Get the path to the addons fiels backup
        public static string GetBackupPathForMod(string addonName)
        {
            return Path.Combine(new string[] 
            {
                MeldiiSettings.Self.FirefallInstallPath,
                ModDataStoreReltivePath,
                addonName
            }) + AddonBackupPostfix;
        }

        // Check if the path is safe
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
                    MessageBox.Show("EnableMelderProtocol");

                    RegistryKey key = Registry.ClassesRoot.CreateSubKey("melder");
                    key.SetValue("", "URL:Melder Protocol");
                    key.SetValue("URL Protocol", "");
                    key.CreateSubKey("DefaultIcon").SetValue("", "Meldii.exe,1");
                    key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");
                    key.Close();
                }
                catch (Exception e)
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
                    MessageBox.Show("EnableMelderProtocol");

                    Registry.ClassesRoot.DeleteSubKeyTree("melder");
                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
