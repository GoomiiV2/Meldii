using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Meldii.AddonProviders;

namespace Meldii.Views
{
    public class AddonMetaData : INotifyPropertyChanged
    {
        #region Varables
        public bool _IsEnabled; // If the addon is in the addon folder
        public bool _IsNotSuported = false;
        public string Name { get; set; }
        public bool IsAddon; // If its an addon or a mod
        private string _Version; // Current ver
        private string _AvailableVersion = "??"; // Newest
        private bool _IsUptoDate;
        public string Author { get; set; }
        public string Description { get; set; }
        public string Destination { get; set; }
        public string AddonPage { get; set; } // eg. Forum post
        public string Patch { get; set; }
        public AddonProviderType ProviderType = AddonProviderType.FirefallForums;
        public List<string> InstalledFilesList;
        public List<string> RemoveFilesList;
        public List<string> IngoreFileList;
        public string ZipName { get; set; }
        public bool IsPendingDelete = false;
        public string DownloadURL = null;
        public bool _IsUpdating = false;

        private string _UpdateURL;
        public string UpdateURL
        {
            get
            {
                if (String.IsNullOrEmpty(_UpdateURL))
                    return AddonPage;
                return _UpdateURL;
            }

            set
            {
                _UpdateURL = value;
            }
        }

        #endregion

        #region Ui Binding Helpers
        public string UiVersion // the version string to show on the ui
        {
            get
            {
                return String.Format("(v{0} / {1})", Version, AvailableVersion);
            }
        }

        public string UiNameAndVersion
        {
            get
            {
                return String.Format("{0} (v{1})", Name, Version);
            }
        }

        public string UiAuthor 
        {
            get
            {
                return String.Format("by {0}", Author);
            }
        }

        public string UiType
        {
            get
            {
                return IsAddon ? "Addon" : "Mod";
            }
        }

        public string Version
        { 
            get 
            {
                return _Version;
            }

            set
            {
                _Version = value;
                Update("UiVersion");
                Update("Version");
            }
        }

        public string AvailableVersion
        {
            get
            {
                return _AvailableVersion;
            }

            set
            {
                _AvailableVersion = value;
                Update("UiVersion");
                Update("AvailableVersion");
            }
        }

        public bool IsUptoDate
        {
            get
            {
                return _IsUptoDate && !_IsNotSuported;
            }
            
            set 
            {
                _IsUptoDate = value;
                Update("IsUptoDate");
                Update("ShouldShowDLIcon");
            }
        }

        public bool ShouldShowDLIcon
        {
            get
            {
                return _AvailableVersion != "??";
            }

            set { }
        }

        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }

            set
            {
                _IsEnabled = value;

                if (Statics.AddonManager != null)
                {
                    if (_IsEnabled)
                        Statics.AddonManager.InstallAddon(this);
                    else
                        Statics.AddonManager.UninstallAddon(this);
                }

                Update("IsEnabled");
            }
        }

        public bool IsNotSuported
        {
            get
            {
                return _IsNotSuported;
            }

            set
            {
                _IsNotSuported = value;
                Update("IsNotSuported");
            }
        }

        public bool IsUpdating
        {
            get
            {
                return _IsUpdating;
            }

            set
            {
                _IsUpdating = value;
                Update("IsUpdating");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Update(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public bool CheckIfAddonOrMod()
        {
            Debug.WriteLine("{3}: {0} {1} {2}", RemoveFilesList.Count, Destination, Destination == null, Name);

            if (RemoveFilesList != null && RemoveFilesList.Count == 0 && Destination == null)
            {
                IsAddon = true;
            }
            else
            {
                IsAddon = false;
            }

            return IsAddon;
        }

        #region Melder Ini Loading and saving
        // Arkii: Why did you choose ini Dax :<
        public void ReadFromIni(TextReader reader)
        {
            InstalledFilesList = new List<string>();
            RemoveFilesList = new List<string>();
            IngoreFileList = new List<string>();
            IngoreFileList.Add("melder_info.ini");
            bool isMultilineDesc = false;

            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                int sep = line.IndexOf('=');
                if (sep != -1)
                {
                    string name = line.Substring(0, sep).Trim().ToLower();
                    string value = line.Substring(sep + 1).Trim();
                    isMultilineDesc = false;

                    switch (name)
                    {
                        case "title":
                            Name = value;
                            break;
                        case "author":
                            Author = value;
                            break;
                        case "version":
                            Version = value;
                            break;
                        case "compatible":
                        case "patch":
                            Patch = value;
                            break;
                        case "url":
                            AddonPage = value;
                            break;
                        case "updateurl":
                            UpdateURL = value;
                            break;
                        case "desc":
                        case "description":
                            isMultilineDesc = true;
                            Description = value;
                            break;
                        case "dest":
                        case "destination":
                            Destination = value;
                            break;
                        case "providertype":
                            try { ProviderType = (AddonProviderType)Enum.Parse(typeof(AddonProviderType), value, true); }
                            catch (Exception) { ProviderType = AddonProviderType.FirefallForums; }
                            break;
                        case "installed":
                            InstalledFilesList.Add(value);
                            break;
                        case "remove":
                            RemoveFilesList.Add(value);
                            break;
                        case "ignore":
                            IngoreFileList.Add(value);
                            break;
                    }
                }
                else if (isMultilineDesc)
                {
                    Description += "\n" + line;
                }
            }

            // Fix up some old addons or misconfigured ones
            if (Destination != null && Statics.FixPathSlashes(Destination).Contains(Statics.DefaultAddonLocation) || Destination == "" )
            {
                Destination = null;
            }

            CheckIfAddonOrMod();
        }

        public void WriteToIni(string path)
        {
            using(TextWriter ini = new StreamWriter(File.OpenWrite(path)))
            {
                ini.WriteLine(string.Format("; Meldii generated installation info for {0}.\n", Name));

                ini.WriteLine(string.Format("title={0}", Name));
                ini.WriteLine(string.Format("author={0}", Author));
                ini.WriteLine(string.Format("version={0}", Version));
                ini.WriteLine(string.Format("patch={0}", Patch));
                ini.WriteLine(string.Format("url={0}", AddonPage));
                ini.WriteLine(string.Format("destination={0}", Destination));
                ini.WriteLine(string.Format("description={0}", Description));
                ini.WriteLine(string.Format("providertype={0}", ProviderType.ToString()));

                if (!String.IsNullOrEmpty(_UpdateURL))
                    ini.WriteLine(string.Format("updateurl={0}", _UpdateURL));

                foreach (string str in InstalledFilesList)
                {
                    ini.WriteLine(string.Format("installed={0}", str));
                }

                foreach (string str in RemoveFilesList)
                {
                    ini.WriteLine(string.Format("remove={0}", str));
                }

                foreach (string str in IngoreFileList)
                {
                    ini.WriteLine(string.Format("ignore={0}", str));
                }

            }
        }
        #endregion
    }
}