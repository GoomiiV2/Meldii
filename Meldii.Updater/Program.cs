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
            if (args.Length >= 1)
            {
                string url = args[0];

                Console.WriteLine("---- Meldii updater ----\n");
                Console.WriteLine("Waiting for Meldii to close...");

                Thread.Sleep(5000);

                // Remove Meldii
                if (File.Exists(MeldiiName))
                {
                    try
                    {
                        File.Delete(MeldiiName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error trying to remove "+MeldiiName);
                        return;
                    }
                }

                using (WebClient Client = new WebClient())
                {
                    Console.WriteLine("Downloading Update");
                    Client.DownloadFile(url, MeldiiName);
                    Console.WriteLine("Update downloaded :>");
                }

                Console.WriteLine("Starting Medlii now");
                Process.Start("Meldii.exe");
            }
        }
    }
}
