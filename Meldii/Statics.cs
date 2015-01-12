using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meldii
{
    public class Statics
    {
        public static string MeldiiAppData = "";
        public static string SettingsPath = "";
        public static bool IsFirstRun = true;

        public static void InitStaticData()
        {
            MeldiiAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Meldii");
            SettingsPath = Path.Combine(MeldiiAppData, "settings.json");

            if (!Directory.Exists(MeldiiAppData))
            {
                Directory.CreateDirectory(MeldiiAppData);
            }

            if (!File.Exists(SettingsPath))
            {
                IsFirstRun = true;
            }
        }
    }
}
