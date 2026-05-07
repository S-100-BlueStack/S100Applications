using NLog;

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
