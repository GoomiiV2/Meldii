using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Meldii.DataStructures
{
    public class MeldiiSettings
    {
        public static MeldiiSettings Self = new MeldiiSettings();

        public string FirefallInstallPath = "";
        public string AddonLibaryPath = "";
        public bool IsMelderProtcolEnabled = false;

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
            catch (Exception e)
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
            catch (Exception e)
            {
                Debug.WriteLine("Error saving settings.json");
            }
        }
    }
}
