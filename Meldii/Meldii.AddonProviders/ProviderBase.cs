using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.DataStructures;
using Meldii.Views;

namespace Meldii.AddonProviders
{
    // Arkii: SpaceX launch just went btw 10/01/2015
    public class ProviderBase
    {
        public static string tempDlDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Properties.Settings.Default.TempStorageLoc);

        public ProviderBase()
        {
            Directory.CreateDirectory(tempDlDir);
        }

        public MelderInfo GetMelderInfo(string url)
        {
            try
            {
                MelderInfo Info = new MelderInfo();
                Info.IsNotSuported = false;

                WebClient client = new WebClient();
                client.Headers["User-Agent"] = Properties.Settings.Default.Useragent;

                string PageData = client.DownloadString(url);
                if (PageData != null)
                {
                    // Check if not suported
                    string StartTag = "<span class=\"prefix prefixGray\">";
                    int start = PageData.IndexOf(StartTag);
                    if (start != -1 && start < PageData.Length)
                    {
                        string tag = PageData.Substring(start + StartTag.Length);
                        int end = tag.IndexOf("</span>");
                        tag = tag.Substring(0, end);
                        tag = tag.Replace("\n", "").Replace("\r", "").Trim();

                        Info.IsNotSuported = (tag.Contains("Not Supported"));
                    }

                    // Melder info
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
                                    case "providertype":
                                        Info.ProviderType = (AddonProviderType)Enum.Parse(typeof(AddonProviderType), value, true);
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

        public virtual void Update(AddonMetaData addon)
        {

        }

        public virtual void DownloadAddon(string url)
        {
            
        }

        public virtual void CopyUpdateToLibrary(string oldFile, string newFile)
        {
            if (File.Exists(oldFile))
                File.Delete(oldFile);

            File.Move(newFile, oldFile);
        }
    }
}
