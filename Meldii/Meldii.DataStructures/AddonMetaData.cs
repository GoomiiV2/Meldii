using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Meldii.AddonProviders;

namespace Meldii.Views
{
    public class AddonMetaData : INotifyPropertyChanged
    {
        #region Varables
        public bool IsEnabled { get; set; } // If the addon is in the addon folder
        public string Name { get; set; }
        public bool IsAddon; // If its an addon or a mod
        private string _Version; // Current ver
        private string _AvailableVersion; // Newest
        private bool _IsUptoDate;
        public string Author { get; set; }
        public string Description { get; set; }
        public string Destnation { get; set; }
        public string AddonPage { get; set; } // eg. Fourm post
        public string Patch { get; set; }
        public AddonProviderType ProviderType;
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
                return _IsUptoDate;
            }
            
            set 
            {
                _IsUptoDate = value;
                Update("IsUptoDate");
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

        #region Melder Ini Loading and saving
        // Arkii: Why did you choose ini Dax :<
        public void ReadFromIni(TextReader reader)
        {
            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                if (line.Contains("="))
                {
                    string[] args = line.Split('=');
                    string name = args[0].Trim().ToLower();
                    string value = args[1].Trim();

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
                        case "desc":
                        case "description":
                            Description = value;
                            break;
                        case "dest":
                        case "destnation":
                            Destnation = value;
                            break;
                        case "providerType":
                            ProviderType = (AddonProviderType)Enum.Parse(typeof(AddonProviderType), value);
                            break;
                    }
                }
            }

            // No ProviderType, assume Firefall fourm download
            ProviderType = AddonProviderType.FirefallFourms;
        }
        #endregion
    }
}