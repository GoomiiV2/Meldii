using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace Meldii
{
    public class Program
    {
        [STAThreadAttribute]
        public static void Main(string[] arguments)
        {
            Func<bool> net45 = () => {
                // Class "ReflectionContext" exists from .NET 4.5 onwards.
                return Type.GetType("System.Reflection.ReflectionContext", false) != null;
            };

            // .NET 4.5 is an in-place upgrade to .NET 4.0.  For some reason some .NET 4.0 systems are
            // running Meldii, even though we are targetting .NET 4.5.  This will check to see if we are
            // being run on a system that has the .NET 4.5 framework installed.
            if (!net45())
            {
                if (MessageBox.Show("Meldii only supports .NET 4.5 or greater.\nDo you wish to download .NET 4.5?", "Runtime Version Error", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=42643");
                return;
            }

            string args = "";
            for (int i = 0; i < arguments.Length; i++)
                args += arguments[i] + " ";
            args = args.Trim();

            Statics.LaunchArgs = args;

            ParseProtcol(args);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ErrorHandler);
            App.Main();
        }

        static void ParseProtcol(string args)
        {
            try
            {
                if (args == "--enable-one-click")
                {
                    try
                    {
                        RegistryKey key = Registry.ClassesRoot.CreateSubKey("melder", RegistryKeyPermissionCheck.ReadWriteSubTree);
                        key.SetValue("", "URL:Melder Protocol");
                        key.SetValue("URL Protocol", "");
                        key.CreateSubKey("DefaultIcon").SetValue("", "Meldii.exe,1");
                        key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");
                        key.Close();
                    }
                    catch (Exception)
                    {
                        Environment.Exit(1);
                    }

                    Environment.Exit(0);
                }
                else if (args == "--disable-one-click")
                {
                    try
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree("melder");
                    }
                    catch (Exception)
                    {
                        Environment.Exit(1);
                    }

                    Environment.Exit(0);
                }
                else
                {
                    Match match = Regex.Match(args, Statics.MelderProtcolRegex);
                    if (match.Success)
                    {
                        string action = match.Groups[1].Value;
                        string provider = match.Groups[2].Value;
                        string url = match.Groups[3].Value;

                        if (action == "download")
                        {
                            if (provider == "forum") // Backwards Melder compat
                            {
                                Statics.OneClickInstallProvider = AddonProviders.AddonProviderType.FirefallForums;
                                Statics.OneClickAddonToInstall = url;
                            }
                            else // New stuff
                            {
                                try
                                {
                                    Statics.OneClickInstallProvider = (AddonProviders.AddonProviderType)Enum.Parse(typeof(AddonProviders.AddonProviderType), provider, true);
                                    Statics.OneClickAddonToInstall = url;
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {

            }
        }

        static void ErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show(e.StackTrace, e.Message);
            string[] lines = 
            {
                ".Net Runtime Version: " + Environment.Version.ToString(),
                "OS: " + Environment.OSVersion.ToString(),
                "Source: " + e.Source,
                "Target: " + e.TargetSite,
                "Message: " + e.Message,
                "\n",
                e.StackTrace,
                "\n"
            };

            System.IO.File.WriteAllLines(@"Meldii Errors make Arkii sad.txt", lines);
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
