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
            MessageBox.Show(e.Exception.StackTrace, e.Exception.Message);
            string[] lines = 
            {
                ".Net Runtime Version: " + Environment.Version.ToString(),
                "OS: " + Environment.OSVersion.ToString(),
                "Source: " + e.Exception.Source,
                "Target: " + e.Exception.TargetSite,
                "Message: " + e.Exception.Message,
                "\n",
                e.Exception.StackTrace,
                "\n"
            };

            System.IO.File.WriteAllLines(@"Meldii Crash more tears.txt", lines);
        }
    }
}
