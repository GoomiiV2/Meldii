using System.Windows;
using System.Windows.Forms;
using Meldii.Views;

namespace Meldii.Windows
{
    /// <summary>
    /// Interaction logic for SettingsFlyout.xaml
    /// </summary>
    public partial class SettingsFlyout : System.Windows.Controls.UserControl
    {
        private SettingsView View = null;

        public SettingsFlyout()
        {
            InitializeComponent();
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

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            View = (SettingsView)DataContext;
        }
    }
}
