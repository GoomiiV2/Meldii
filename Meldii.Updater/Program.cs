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
            // Attempt to close all Meldii processes.
            // This will try every second for 5 seconds.
            int i = 0;
            while (i < 5)
            {
                var processes = Process.GetProcessesByName("meldii");
                processes = processes.Where(x => x.ProcessName != "Meldii.Updater.exe").ToArray();
                if (processes.Length == 0)
                    break;

                // Kill each process.
                foreach (var process in processes)
                    process.Kill();

                // Wait 1 second.
                Thread.Sleep(1000);
                ++i;
            }

            // Failed to close Meldii for whatever reason.
            if (i == 5)
            {
                Console.WriteLine("Error trying to close " + MeldiiName);
                return;
            }

            // Remove Meldii
            if (File.Exists(MeldiiName))
            {
                try
                {
                    File.Delete(MeldiiName);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error trying to remove " + MeldiiName);
                    return;
                }
            }

            File.Move("Meldii_New.exe", MeldiiName);

            Process.Start(MeldiiName);
        }
    }
}
