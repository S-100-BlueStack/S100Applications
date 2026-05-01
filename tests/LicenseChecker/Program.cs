namespace LicenseChecker
{
    internal class Program
    {
        static int Main(string[] args) {
            Console.Clear();
            Console.WriteLine("Hello, World!");


            Console.WriteLine();

            try {
                Console.WriteLine("ArcGIS.Core.Hosting.Host.Initialize()");
                ArcGIS.Core.Hosting.Host.Initialize();

            }
            catch(System.Exception ex) {
                Console.WriteLine(ex.ToString());
                return -1;
            }

            Console.WriteLine();
            Console.WriteLine("All systems ready to go!");
            return 0;
        }
    }
}
