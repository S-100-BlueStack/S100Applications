using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace S100Framework.Applications;

internal static class ImportLogCategories
{
    internal const string Property = "ImportCategory";
    internal const string DataObject = "DataObject";
    internal const string DataTotalCount = "DataTotalCount";
    internal const string DataError = "DataError";

    internal static bool Is(LogEvent logEvent, string category) =>
        logEvent.Properties.TryGetValue(Property, out var value) &&
        value is ScalarValue { Value: string text } && text == category;
}

public static class CustomLogLevels
{
    // Categories route reports; severity retains Serilog's supported meaning.
    public static void DataTotalCount(this ILogger logger, string tablename, int totalCount, int convertedCount) =>
        logger.ForContext(ImportLogCategories.Property, ImportLogCategories.DataTotalCount)
            .Information("{tablename};{totalCount};{convertedCount}", tablename, totalCount, convertedCount);

    public static void DataObject(this ILogger logger, long objectId, string tableName, string longname, string destination) =>
        logger.ForContext(ImportLogCategories.Property, ImportLogCategories.DataObject)
            .Information("{ObjectId};{TableName};{LongName};{Destination}", objectId, tableName, longname, destination);

    public static void DataObject(ILogger logger, string fileName, string message) =>
        logger.ForContext(ImportLogCategories.Property, ImportLogCategories.DataObject)
            .ForContext("TableName", fileName).Information("{ReportMessage}", message);

    public static void DataError(this ILogger logger, long objectId, string tableName, string longname, string message) =>
        logger.ForContext(ImportLogCategories.Property, ImportLogCategories.DataError)
            .Error("{ObjectId};{TableName};{LongName};{Message}", objectId, tableName, longname, message);
}

public sealed class DynamicFileSink : ILogEventSink, IDisposable
{
    private readonly string _logDirectory;
    private readonly object _gate = new();
    private readonly Dictionary<string, StreamWriter> _writers = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;
    private const int MaximumOpenWriters = 64;

    public DynamicFileSink(string logDirectory) {
        this._logDirectory = logDirectory;
        Directory.CreateDirectory(logDirectory);
    }

    public void Emit(LogEvent logEvent) {
        var table = logEvent.Properties.TryGetValue("TableName", out var value) &&
            value is ScalarValue { Value: string name } ? name : "Unknown";
        // Hash arbitrary names so database metadata cannot escape the report directory.
        string safeName = table.Length is > 0 and <= 80 && table.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-')
            ? table
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(table)));
        string path = Path.Combine(this._logDirectory, $"ImporterNIS_DataObject_{safeName}.log");
        lock (this._gate) {
            ObjectDisposedException.ThrowIf(this._disposed, this);
            if (!this._writers.TryGetValue(path, out var writer)) {
                // Bound handles even when a source has many distinct table names.
                if (this._writers.Count >= MaximumOpenWriters) {
                    var oldest = this._writers.First();
                    oldest.Value.Dispose();
                    this._writers.Remove(oldest.Key);
                }
                writer = new StreamWriter(path, append: true, new UTF8Encoding(false)) { AutoFlush = true };
                this._writers.Add(path, writer);
            }
            writer.WriteLine(logEvent.RenderMessage(CultureInfo.InvariantCulture));
        }
    }
    public void Dispose() {
        lock (this._gate) {
            if (this._disposed) return;
            foreach (var writer in this._writers.Values) writer.Dispose();
            this._writers.Clear();
            this._disposed = true;
        }
    }

}

internal static class Logger
{
    private static Serilog.Core.Logger? _logger;
    internal static ILogger Current => _logger ?? throw new InvalidOperationException("Logging has not been configured.");
    internal static string LogDir { get; private set; } = string.Empty;

    internal static void Configure(bool verbose, string? logDirectory = null) {
        if (_logger is not null) throw new InvalidOperationException("Logging is already configured.");
        LogDir = logDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "S-100 BlueStack", "ImporterNIS", $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(LogDir);
        _logger = new LoggerConfiguration()
            .MinimumLevel.Is(verbose ? LogEventLevel.Verbose : LogEventLevel.Information)
            .Enrich.WithExceptionData()
            .WriteTo.Logger(lc => lc.Filter.ByExcluding(e => e.Properties.ContainsKey(ImportLogCategories.Property))
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(LogDir, "ImporterNIS_System.log"),
                    encoding: new UTF8Encoding(false),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}| [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => ImportLogCategories.Is(e, ImportLogCategories.DataObject))
                .WriteTo.Sink(new DynamicFileSink(LogDir)))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => ImportLogCategories.Is(e, ImportLogCategories.DataTotalCount))
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(LogDir, "ImporterNIS_DataTotalCount.log"), encoding: new UTF8Encoding(false),
                    outputTemplate: "{Message:lj}{NewLine}"))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => ImportLogCategories.Is(e, ImportLogCategories.DataError))
                .WriteTo.File(Path.Combine(LogDir, "ImporterNIS_DataError.log"), encoding: new UTF8Encoding(false),
                    outputTemplate: "{Message:lj}{NewLine}"))
            .CreateLogger();
    }

    internal static void TryLogFatal(Exception exception) => _logger?.Fatal(exception, "Import failed");
    internal static void Close() {
        _logger?.Dispose();
        _logger = null;
    }
}
