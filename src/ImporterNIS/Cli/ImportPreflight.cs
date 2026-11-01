using ArcGIS.Core.Data;
using S100Framework.ImporterNIS.Core;
using System.Text.RegularExpressions;

namespace S100Framework.Applications;

internal static class ImportPreflight
{
    internal static void ValidateConnections(ImportOptions options)
    {
        // Open every workspace before the legacy conversion engine can delete any rows.
        using var source = GeodatabaseFactory.Create(options.Source!)();
        using var metadata = source.OpenDataset<FeatureClass>(source.GetName("MetaDataA")
            ?? throw new InvalidOperationException("The source has no MetaDataA feature class."));
        var targetPath = options.InitializeTarget
            ? Path.Combine(AppContext.BaseDirectory, "s100ed18.geodatabase")
            : options.Target!;
        using var target = GeodatabaseFactory.Create(targetPath)();
        ValidateTarget(target);
        if (!string.IsNullOrWhiteSpace(options.S128)) {
            using var s128 = GeodatabaseFactory.Create(options.S128)();
            ValidateTarget(s128);
        }
    }

    private static void ValidateTarget(Geodatabase database)
    {
        foreach (string name in new[] { "point", "pointset", "curve", "surface" }) {
            using var featureClass = database.OpenDataset<FeatureClass>(database.GetName(name)
                ?? throw new InvalidOperationException($"Target feature class is missing: {name}"));
        }
        foreach (string name in new[] { "informationtype", "featuretype" }) {
            using var table = database.OpenDataset<Table>(database.GetName(name)
                ?? throw new InvalidOperationException($"Target table is missing: {name}"));
        }
    }
}
