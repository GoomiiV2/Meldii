using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Meldii.DataStructures
{
    public class MeldiiSettings
    {
        public static MeldiiSettings Self = new MeldiiSettings();

        public string FirefallInstallPath = "";
        public string AddonLibaryPath = "";
        public bool IsMelderProtocolEnabled = false;
        public bool CloseMeldiiOnFirefallLaunch = true;
        public string Theme;
        public string ThemeAccent;
        public bool CheckForPatchs = true;

        public void Load()
        {
            try
            {
                using (FileStream fs = File.Open(Statics.SettingsPath, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MeldiiSettings));
                    Self = (MeldiiSettings)serializer.ReadObject(fs);
                    Debug.WriteLine("AddonLibaryPathAddonLibaryPath: " + Self.AddonLibaryPath);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error loading settings.json");
            }
        }

        public void Save()
        {
            try
            {
                using (FileStream fs = File.Open(Statics.SettingsPath, FileMode.Create))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MeldiiSettings));
                    serializer.WriteObject(fs, this);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error saving settings.json");
            }
        }
    }
}
