using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Meldii.AddonProviders;
using Meldii.DataStructures;
using Meldii.Views;
using Meldii.Windows;

namespace Meldii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainViewModel ViewModel = new MainViewModel();
        AddonManager AddonManager = null;
        public bool IsSettingsWindowOpen = false;
        public bool IsHelpWindowOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            Statics.InitStaticData();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MeldiiSettings.Self.Load();
            DataContext = ViewModel;
            AddonManager = new AddonManager(ViewModel);
            Statics.AddonManager = AddonManager;
        }

        private void Btt_OpenAddonPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OnOpenAddonPage();
        }

        private void Btt_OpenSettings(object sender, RoutedEventArgs e)
        {
            if (!IsSettingsWindowOpen)
            {
                SettingsWindow settingsWindow = new SettingsWindow(this);
                settingsWindow.Show();
                IsSettingsWindowOpen = true;
            }
        }

        private void Btt_OpenHelp(object sender, RoutedEventArgs e)
        {
            if (!IsHelpWindowOpen)
            {
                HelpWindow settingsWindow = new HelpWindow(this);
                settingsWindow.Show();
                IsHelpWindowOpen = true;
            }
        }

        private void AddonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.OnSelectedAddon(AddonList.SelectedIndex);
        }
    }
}
