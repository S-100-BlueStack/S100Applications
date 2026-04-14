using NLog;

using System;

namespace NuvionPro
{
    internal static class Logger
    {
        public static ILogger Current => _logger;

        private static readonly NLog.ILogger _logger;

        private const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}| [{Level:u3}] {Message:lj} {NewLine}{Exception}";

        //public static ILogger WithClassAndMethodNames<T>(this ILogger logger, [CallerMemberName] string memberName = "")
        //{
        //    var className = typeof(T).Name;
        //    return logger.ForContext("ClassName", className).ForContext("MethodName", memberName);
        //}

        static Logger() {
            NLog.LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToFile(
                    fileName: System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"NuvionPro", "nuvionpro-developer.log"),
                    encoding: System.Text.Encoding.UTF8,
                    maxArchiveDays: 31
                );
            });

            _logger = NLog.LogManager.GetLogger("NuvionPro");
        }
    }
}
