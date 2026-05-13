using ABI.System;
using ArcGIS.Core.Data;
using ArcGIS.Core.SystemCore;
using NLog;
using Uri = System.Uri;

namespace LicenseChecker
{
    internal class Program
    {
        internal class CredentialHandler(string username, string password) : ISignOnHandler
        {
            private readonly string _username = username;
            private readonly string _password = password;

            public void GenerateCredentials(ref SIGNONHANDLERINFO info) {
                string referer = "";
                string token = "";

                Logger.Info("Generating credentials for portal: {ago}", info.agoURL);
                Console.WriteLine($"Generating credentials for portal {info.agoURL}");

                var uri = new Uri(info.agoURL, UriKind.Absolute);

                bool ok = ArcGISSignOn.Instance.SignInWithCredentials(
                    uri, _username, _password, out referer, out token);

                if (!ok)
                    throw new System.Exception($"Sign-in failed for {info.agoURL}");


                Logger.Info("Credentials generated successfully with token: {token}", token);
            }
        }

        static Logger? Logger = default;

        internal static Geodatabase OpenFeatureService(string featureServiceUrl, string portalUrl, string portalUser, string portalPassword) {
            var arcGisSignOn = ArcGISSignOn.Instance;
            var portalUri = new Uri(portalUrl);
            var workspaceUri = new Uri(featureServiceUrl);
            if (arcGisSignOn.IsSignedOn(portalUri))
                arcGisSignOn.SignOut(portalUri);
            arcGisSignOn.SignInWithCredentials(portalUri, portalUser, portalPassword, out var referer, out var token);

            return new Geodatabase(new ServiceConnectionProperties(workspaceUri));
        }

        static int Main(string[] args) {
            NLog.LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToConsole();
            });

            Console.Clear();
            Console.WriteLine("Hello, LicenseChecker!");

            Logger = NLog.LogManager.GetCurrentClassLogger();


            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
                Logger.Fatal(eventArgs.ExceptionObject as System.Exception, "Unhandled exception");
            };


            Logger.Info("ArcGIS.Core.Hosting.Host.Initialize()");
            try {
                Console.WriteLine("ArcGIS.Core.Hosting.Host.Initialize()");
                ArcGIS.Core.Hosting.Host.Initialize(ArcGIS.Core.Hosting.Host.LicenseProductCode.ArcGISPro);
            }
            catch (System.Exception ex) {
                Logger.Error(ex);
                return -1;
            }

            var username = @"PROD\F042096";
            var password = "cqEHrUM4sRHV6N5C9WqK";


            //ArcGISSignOn.Instance.SetSignonHandler(new CredentialHandler(username,password));

            //var result = ArcGISSignOn.Instance.SignInWithCredentials(new Uri("https://nuvion.gst.dk/portal", UriKind.Absolute), username,password, out string referrer, out string token);

            //Logger.Info("Sign-in result: {result}, token: {token}", result, token);

            //Logger.Info("ArcGIS.Core.Hosting.Host.Initialize()");
            //try {
            //    var portal = ArcGISPortalManager.Current.GetPortal(new Uri("https://nuvion.gst.dk/portal", UriKind.Absolute));
            //    if (portal.SignIn().success) {
            //        //Set this portal as my active portal
            //        ArcGISPortalManager.Current.SetActivePortal(portal);
            //    }
            //    var user = portal.GetSignOnUsername();
            //    Logger.Info("user: {user}", user);
            //}
            //catch (System.Exception ex) {
            //    Logger.Error(ex);
            //    return -1;
            //}

            try {
                var featureServiceUri = new Uri("https://nuvion.gst.dk/arcgis/rest/services/MasterPolygon/FeatureServer");

                using var geodatabase = OpenFeatureService(
                    "https://nuvion.gst.dk/arcgis/rest/services/MasterPolygon/FeatureServer",
                    portalUrl: "https://nuvion.gst.dk/portal",
                    portalUser: username,
                    portalPassword: password);

                var manager = geodatabase.GetVersionManager();
                var version = manager.CreateVersion(new VersionDescription($"TEST_{DateTime.Now.ToFileTime()}", "Testing", VersionAccessType.Public));

                Logger.Info("Version created: {versionName}", version.GetName());
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
