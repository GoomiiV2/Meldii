using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Meldii
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.IO.File.WriteAllText(@"Meldii Crash more tears.txt", Program.ParseException(e.Exception));
            MessageBox.Show("Meldii has encountered an error.  Please check the exception logs.", "Error");
        }
    }
}
