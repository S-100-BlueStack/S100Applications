using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace S100Framework.Applications
{
    internal static class Logger
    {
        private static readonly string _dateTimeString = DateTime.Now.ToString("yyyyMMdd");
        private static readonly Serilog.Core.Logger _logger;
        private static readonly string _logDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static ILogger Current => _logger;

        static Logger() {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level < (LogEventLevel)6)
                    .Enrich.WithExceptionData()
                    .WriteTo.File(System.IO.Path.Combine(_logDir, @"S-100 BlueStack", "ExporterYAML", $"{_dateTimeString}.log"),
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    encoding: System.Text.Encoding.GetEncoding("ISO-8859-1"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}| [{Level:u3}] {Message:lj} {NewLine}{Exception}"))

                .WriteTo.Logger(lc => lc
                    .WriteTo.Console())

                .CreateLogger();
        }
    }
}
