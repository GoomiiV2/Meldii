using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meldii.AddonProviders;

namespace Meldii
{
    public class Statics
    {
        public static string MeldiiAppData = "";
        public static string SettingsPath = "";
        public static string AddonsFolder = "";
        public static bool IsFirstRun = true;

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
    }
}
