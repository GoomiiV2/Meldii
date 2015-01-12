using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.DataStructures;

namespace Meldii.AddonProviders
{
    // Arkii: SpaceX launch just went btw 10/01/2015
    public class ProviderBase
    {
        public MelderInfo GetMelderInfo(string url)
        {
            try
            {
                MelderInfo Info = new MelderInfo();

                WebClient client = new WebClient();
                string PageData = client.DownloadString(url);
                if (PageData != null)
                {
                    int trimStart = PageData.IndexOf("[melder_info]") + "[melder_info]".Length;
                    int trimEnd = PageData.IndexOf("[/melder_info]");
                    string MelderData = PageData.Substring(trimStart, trimEnd - trimStart);
                    if (MelderData != null)
                    {
                        string[] values = MelderData.Split(';');
                        foreach (string str in values)
                        {
                            if (str.Length > 1)
                            {
                                string[] args = str.Split('=');
                                string key = args[0].ToLower().Trim();
                                string value = args[1].Trim();

                                switch (key)
                                {
                                    case "version":
                                        Info.Version = value;
                                        break;
                                    case "patch":
                                        Info.Patch = value;
                                        break;
                                    case "dlurl":
                                        Info.Dlurl = value;
                                        break;
                                    case "providerType":
                                        Info.ProviderType = (AddonProviderType)Enum.Parse(typeof(AddonProviderType), value);
                                        break;
                                }
                            }
                        }
                        client.Dispose();
                        return Info;
                    }
                }
                client.Dispose();
                return null;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }
    }
}
