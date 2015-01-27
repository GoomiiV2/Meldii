using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meldii.AddonProviders;
using Meldii.DataStructures;

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
    }
}
