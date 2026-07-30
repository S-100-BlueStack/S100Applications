using System.Text;
using System.Text.Json;
using S100.Iso8211.S57;
using S100.Iso8211.Serialization;
using S100.Iso8211.S101;

namespace S100.Iso8211.Cli;

internal static class Program
{
    private const string Usage = """
        s100json - convert an ISO/IEC 8211 (S-100 / S-101 / S-57) file to YAML or JSON

          s100json <input> [options]

        Options
          -f, --format <format>   yaml (default) or json. When omitted, inferred from the
                                  output file extension, falling back to yaml.
          -o, --out <path>        Output file. Defaults to <input>.yaml, or stdout with "-".
          -m, --mode <mode>       raw       lossless dump of the DDR and every record
                                  features  dataset header + features with geometry (default)
                                  geojson   plain RFC 7946 FeatureCollection
          -p, --product <product> auto (default), s57 or s101. Detected from the DDR.
          --compact               Write minified JSON. Ignored for YAML.
          --big-endian            Decode b* subfields most significant byte first.
          --encoding <name>       Fallback text encoding (default utf-8).
          --feature-types <file>  JSON object mapping feature type or OBJL code -> name.
          --attributes <file>     JSON object mapping attribute code -> name.
          --skip-malformed        Keep going when a record fails to parse.
        """;

    private const int ExitSuccess = 0;
    private const int ExitReadError = 1;
    private const int ExitUsageError = 2;

    internal static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            Console.WriteLine(Usage);
            return ExitSuccess;
        }

        if (!TryParseArguments(args, out ConversionOptions options, out string? error))
        {
            Console.Error.WriteLine(error);
            return ExitUsageError;
        }

        if (!File.Exists(options.InputPath))
        {
            Console.Error.WriteLine($"Input not found: {options.InputPath}");
            return ExitUsageError;
        }

        try
        {
            Convert(options);
            return ExitSuccess;
        }
        catch (Iso8211Exception ex)
        {
            Console.Error.WriteLine($"Not a readable ISO/IEC 8211 file: {ex.Message}");
            return ExitReadError;
        }
    }

    private static void Convert(ConversionOptions options)
    {
        string outputPath = options.ResolvedOutputPath;
        bool toStdout = outputPath == "-";

        using Stream target = toStdout
            ? Console.OpenStandardOutput()
            : File.Create(outputPath);

        using var reader = Iso8211Reader.Open(options.InputPath, options.ReaderOptions);

        if (options.Mode == ConversionMode.Raw)
        {
            Iso8211DocumentWriter.Write(
                reader, target, options.Format, options.Indented, Path.GetFileName(options.InputPath));
            return;
        }

        Iso8211Product product = options.Product ?? ProductDetector.Detect(reader);
        bool geoJsonOnly = options.Mode == ConversionMode.GeoJson;

        int featureCount, spatialCount;
        IReadOnlyList<string> warnings;

        if (product == Iso8211Product.S57)
        {
            var dataset = S57DatasetReader.Read(reader, options.S57Options, Path.GetFileName(options.InputPath));
            S57DocumentWriter.Write(dataset, target, options.Format, options.Indented, geoJsonOnly);
            featureCount = dataset.Features.Count;
            spatialCount = dataset.VectorRecords.Count;
            warnings = dataset.Warnings;
        }
        else
        {
            var dataset = S101DatasetReader.Read(reader, options.S101Options, Path.GetFileName(options.InputPath));
            S101DocumentWriter.Write(dataset, target, options.Format, options.Indented, geoJsonOnly);
            featureCount = dataset.Features.Count;
            spatialCount = dataset.SpatialRecords.Count;
            warnings = dataset.Warnings;
        }

        if (toStdout) return;

        string label = product == Iso8211Product.S57 ? "S-57" : "S-101";
        Console.Error.WriteLine(
            $"{label}: {featureCount} features, {spatialCount} spatial records -> {outputPath}");

        foreach (string warning in warnings.Take(20))
            Console.Error.WriteLine($"  warning: {warning}");

        if (warnings.Count > 20)
            Console.Error.WriteLine($"  ... and {warnings.Count - 20} more warnings");
    }

    private static bool TryParseArguments(string[] args, out ConversionOptions options, out string? error)
    {
        options = new ConversionOptions { InputPath = args[0] };
        error = null;

        for (int i = 1; i < args.Length; i++)
        {
            string argument = args[i];
            switch (argument)
            {
                case "-o" or "--out":
                    if (!TryTakeValue(args, ref i, argument, out string outPath, out error)) return false;
                    options.OutputPath = outPath;
                    break;

                case "-f" or "--format":
                    if (!TryTakeValue(args, ref i, argument, out string format, out error)) return false;
                    switch (format.ToLowerInvariant())
                    {
                        case "yaml" or "yml": options.ExplicitFormat = OutputFormat.Yaml; break;
                        case "json": options.ExplicitFormat = OutputFormat.Json; break;
                        default:
                            error = $"Unknown format '{format}'. Expected yaml or json.";
                            return false;
                    }
                    break;

                case "-p" or "--product":
                    if (!TryTakeValue(args, ref i, argument, out string product, out error)) return false;
                    switch (product.ToLowerInvariant())
                    {
                        case "auto": options.Product = null; break;
                        case "s57" or "s-57": options.Product = Iso8211Product.S57; break;
                        case "s101" or "s-101": options.Product = Iso8211Product.S101; break;
                        default:
                            error = $"Unknown product '{product}'. Expected auto, s57 or s101.";
                            return false;
                    }
                    break;

                case "-m" or "--mode":
                    if (!TryTakeValue(args, ref i, argument, out string mode, out error)) return false;
                    switch (mode.ToLowerInvariant())
                    {
                        case "raw": options.Mode = ConversionMode.Raw; break;
                        case "features": options.Mode = ConversionMode.Features; break;
                        case "geojson": options.Mode = ConversionMode.GeoJson; break;
                        default:
                            error = $"Unknown mode '{mode}'. Expected raw, features or geojson.";
                            return false;
                    }
                    break;

                case "--compact":
                    options.Indented = false;
                    break;

                case "--big-endian":
                    options.ReaderOptions.LittleEndianBinary = false;
                    break;

                case "--skip-malformed":
                    options.ReaderOptions.SkipMalformedRecords = true;
                    break;

                case "--encoding":
                    if (!TryTakeValue(args, ref i, argument, out string encodingName, out error)) return false;
                    try
                    {
                        options.ReaderOptions.DefaultEncoding = Encoding.GetEncoding(encodingName);
                    }
                    catch (ArgumentException)
                    {
                        error = $"Unknown encoding '{encodingName}'.";
                        return false;
                    }
                    break;

                case "--feature-types":
                    if (!TryTakeValue(args, ref i, argument, out string featureTypesPath, out error)) return false;
                    var featureTypes = LoadCodeMap(featureTypesPath);
                    options.S101Options.FeatureTypeNames = featureTypes;
                    options.S57Options.ObjectClassNames = featureTypes;
                    break;

                case "--attributes":
                    if (!TryTakeValue(args, ref i, argument, out string attributesPath, out error)) return false;
                    var attributeNames = LoadCodeMap(attributesPath);
                    options.S101Options.AttributeNames = attributeNames;
                    options.S57Options.AttributeNames = attributeNames;
                    break;

                default:
                    error = $"Unknown option: {argument}";
                    return false;
            }
        }

        return true;
    }

    private static bool TryTakeValue(string[] args, ref int index, string option, out string value, out string? error)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;
            error = $"Option {option} needs a value.";
            return false;
        }

        value = args[++index];
        error = null;
        return true;
    }

    private static IReadOnlyDictionary<string, string> LoadCodeMap(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
            map[property.Name] = property.Value.GetString() ?? property.Name;

        return map;
    }
}

internal enum ConversionMode
{
    Raw,
    Features,
    GeoJson
}

internal sealed class ConversionOptions
{
    public required string InputPath { get; init; }
    public string? OutputPath { get; set; }

    /// <summary>Set only when the user passed --format; otherwise the extension decides.</summary>
    public OutputFormat? ExplicitFormat { get; set; }

    /// <summary>An explicit --format wins, then the output extension, then YAML.</summary>
    public OutputFormat Format => ExplicitFormat
        ?? (OutputPath is null or "-" ? OutputFormat.Yaml : OutputFormats.FromPath(OutputPath));

    /// <summary>The output path the user asked for, or "&lt;input&gt;" plus the format's extension.</summary>
    public string ResolvedOutputPath => OutputPath ?? InputPath + Format.Extension();
    public ConversionMode Mode { get; set; } = ConversionMode.Features;
    public bool Indented { get; set; } = true;
    /// <summary>Null means detect from the DDR.</summary>
    public Iso8211Product? Product { get; set; }

    public Iso8211ReaderOptions ReaderOptions { get; } = new();
    public S101ReaderOptions S101Options { get; } = new();
    public S57ReaderOptions S57Options { get; } = new();
}
