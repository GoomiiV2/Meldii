using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Meldii.Windows
{
    /// <summary>
    /// Interaction logic for SettingsFlyout.xaml
    /// </summary>
    public partial class SettingsFlyout : System.Windows.Controls.UserControl
    {
        public SettingsFlyout()
        {
            InitializeComponent();
        }

        private void Btt_SaveSettings(object sender, RoutedEventArgs e)
        {
            //View.SaveSettings();
        }

        private void Btt_FirefallPathFind(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.SelectedPath = View.FirefallInstall;
            var res = fbd.ShowDialog();
            //View.FirefallInstall = fbd.SelectedPath;
        }

        private void Btt_AddonLibFind(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
           // fbd.SelectedPath = View.AddonLibaryPath;
            var res = fbd.ShowDialog();
            //View.AddonLibaryPath = fbd.SelectedPath;
        }
    }
}
