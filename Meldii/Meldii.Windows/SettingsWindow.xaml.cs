using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace Meldii.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private MainWindow MainWindow = null;

        public SettingsWindow(MainWindow _MainWindow)
        {
            MainWindow = _MainWindow;
            //IsHelpWindowOpen = f
            InitializeComponent();
        }

        void CancelEventHandler(object sender, CancelEventArgs e)
        {
            MainWindow.IsSettingsWindowOpen = false;
        }
    }
}
