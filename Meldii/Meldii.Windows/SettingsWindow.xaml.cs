using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Meldii.Views;

namespace Meldii.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private MainWindow MainWindow = null;
        private SettingsView View = new SettingsView();

        public SettingsWindow(MainWindow _MainWindow)
        {
            MainWindow = _MainWindow;
            InitializeComponent();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = View;
        }

        void CancelEventHandler(object sender, CancelEventArgs e)
        {
            MainWindow.IsSettingsWindowOpen = false;
        }

        private void Btt_SaveSettings(object sender, RoutedEventArgs e)
        {
            View.SaveSettings();
        }

        private void Btt_FirefallPathFind(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = View.FirefallInstall;
            var res = fbd.ShowDialog();
            View.FirefallInstall = fbd.SelectedPath;
        }

        private void Btt_AddonLibFind(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = View.AddonLibaryPath;
            var res = fbd.ShowDialog();
            View.AddonLibaryPath = fbd.SelectedPath;
        }
    }
}
