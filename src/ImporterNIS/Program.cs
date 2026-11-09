using CommandLine;
using ICSharpCode.SharpZipLib.Zip;
using S100Framework.ImporterNIS.Core;

namespace S100Framework.Applications;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args) {
        using var parser = new Parser(settings => settings.HelpWriter = Console.Error);
        var parsed = parser.ParseArguments<ImportOptions>(args);
        if (parsed is NotParsed<ImportOptions> failure)
            return failure.Errors.All(e => e is HelpRequestedError or VersionRequestedError) ? 0 : 2;

        var options = ((Parsed<ImportOptions>)parsed).Value;
        try {
            ImportOptionsValidator.ValidateOptions(options);
            Logger.Configure(options.Verbose);
            ArcGIS.Core.Hosting.Host.Initialize();
            ImportPreflight.ValidateConnections(options);

            // Conversion caches still have process lifetime; keep one import on this STA thread.
            var scamin = Path.Combine(AppContext.BaseDirectory, "scamin.gdb");
            if (!Directory.Exists(scamin))
                new FastZip().ExtractZip(Path.Combine(AppContext.BaseDirectory, "scamin.gdb.zip"), scamin, null);
            if (options.InitializeTarget)
                File.Copy(Path.Combine(AppContext.BaseDirectory, "s100ed18.geodatabase"),
                    WorkspaceAddress.Parse(options.Target!).Value, overwrite: false);

            return ImporterNIS.Load(GeodatabaseFactory.Create(options.Target!), options) ? 0 : 1;
        }
        catch (Exception exception) {
            Console.Error.WriteLine($"Import failed: {exception.Message}");
            Logger.TryLogFatal(exception);
            return 1;
        }
        finally {
            Logger.Close();
        }
    }
}
