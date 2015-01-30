﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.AddonProviders;
using Meldii.DataStructures;

namespace Meldii.Views
{
    public class HelpView : INotifyPropertyChanged
    {
        private string _MI_AddonVersion = "1.0";
        private string _MI_FirefallPatch = Statics.GetFirefallPatchData().build;
        private AddonProviderType _MI_Provider = AddonProviderType.FirefallFourms;
        private string _MI_Result = "";
        private string _MI_DLURL = "";

        public HelpView()
        {

        }

        public string MI_AddonVersion
        {
            get
            {
                return _MI_AddonVersion;
            }

            set
            {
                _MI_AddonVersion = value;
                NotifyPropertyChanged("MI_Result");
                NotifyPropertyChanged("MI_AddonVersion");
            }
        }

        public string MI_FirefallPatch
        {
            get
            {
                return _MI_FirefallPatch;
            }

            set
            {
                _MI_FirefallPatch = value;
                NotifyPropertyChanged("MI_Result");
                NotifyPropertyChanged("MI_FirefallPatch");
            }
        }

        public AddonProviderType MI_Provider
        {
            get
            {
                return _MI_Provider;
            }

            set
            {
                _MI_Provider = value;
                NotifyPropertyChanged("MI_Result");
                NotifyPropertyChanged("MI_Provider");
            }
        }

        public string MI_DLURL
        {
            get
            {
                return _MI_DLURL;
            }

            set
            {
                _MI_DLURL = value;
                NotifyPropertyChanged("MI_Result");
                NotifyPropertyChanged("MI_DLURL");
            }
        }

        public string MI_Result
        {
            get
            {
                return GetMedlerInfo();
            }
        }


        public static string AssemblyVersion
        {
            get
            {
                return "Version: " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            }
        }

        public string GetMedlerInfo()
        {
            string info = "[center][url={0}?id={1}][img]{2}[/img][/url][size=1][color=#161C1C][melder_info]version={3};patch={4};dlurl={1};providertype={5}[/melder_info][/color][/size][/center]";
            info = string.Format(info,
                Properties.Settings.Default.MI_HostURL, // host
                _MI_DLURL, // Downlaod url
                Properties.Settings.Default.MI_HostImgURL, // img
                _MI_AddonVersion, // addon version
                _MI_FirefallPatch, // Firefall Version
                _MI_Provider.ToString());// Provider
            return info;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}