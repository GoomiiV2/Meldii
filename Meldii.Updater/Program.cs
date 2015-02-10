using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meldii.Updater
{
    class Program
    {
        static string MeldiiName = "Meldii.exe";

        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            // Remove Meldii
            if (File.Exists(MeldiiName))
            {
                try
                {
                    File.Delete(MeldiiName);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error trying to remove "+MeldiiName);
                    return;
                }
            }

            File.Move("Meldii_New.exe", MeldiiName);

            Process.Start(MeldiiName);
        }
    }
}
