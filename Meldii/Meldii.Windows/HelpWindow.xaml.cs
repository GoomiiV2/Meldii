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
    public partial class HelpWindow : MetroWindow
    {
        private MainWindow MainWindow = null;

        public HelpWindow(MainWindow _MainWindow)
        {
            MainWindow = _MainWindow;

            InitializeComponent();
        }

        void CancelEventHandler(object sender, CancelEventArgs e)
        {
            MainWindow.IsHelpWindowOpen = false;
        }
    }
}
