using ABI.System;
using ArcGIS.Desktop.Core;
using NLog;
using Uri = System.Uri;

namespace LicenseChecker
{
    internal class Program
    {
        static int Main(string[] args) {
            NLog.LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToConsole();
            });

            Console.Clear();
            Console.WriteLine("Hello, LicenseChecker!");


            var Logger = NLog.LogManager.GetCurrentClassLogger();

            Logger.Info("ArcGIS.Core.Hosting.Host.Initialize()");
            try {
                var portal = ArcGISPortalManager.Current.GetPortal(new Uri("https://nuvion.gst.dk/portal", UriKind.Absolute));
                if (portal.SignIn().success) {
                    //Set this portal as my active portal
                    ArcGISPortalManager.Current.SetActivePortal(portal);
                }
                var user = portal.GetSignOnUsername();
                Logger.Info("user: {user}", user);
            }
            catch (System.Exception ex) {
                Logger.Error(ex);
                return -1;
            }


            Logger.Info("ArcGIS.Core.Hosting.Host.Initialize()");
            try {
                Console.WriteLine("ArcGIS.Core.Hosting.Host.Initialize()");
                ArcGIS.Core.Hosting.Host.Initialize(ArcGIS.Core.Hosting.Host.LicenseProductCode.ArcGISPro);
            }
            catch (System.Exception ex) {
                Logger.Error(ex);                
                return -1;
            }

            Logger.Info("All systems ready to go!");
            return 0;
        }
    }
}
