using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Meldii.DataStructures
{
    public class FirefallPatchData
    {
        public string build;
        public string environment;
        public string region;
        public string patch_level;
        public bool error;

        public static FirefallPatchData Create(Stream data)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FirefallPatchData));
                return (FirefallPatchData)serializer.ReadObject(data);
            }
            catch (Exception)
            {
                FirefallPatchData nullPatchData =  new FirefallPatchData();
                nullPatchData.build = "none-0";
                nullPatchData.environment = "none";
                nullPatchData.region = "none";
                nullPatchData.patch_level = "0";
                return nullPatchData;
            }
        }

        public static FirefallPatchData CreateError()
        {
            FirefallPatchData r = new FirefallPatchData();
            r.error = true;
            return r;
        }
    }
}
