using ArcGIS.Core.Data;
using S100Framework.ImporterNIS.Core;

namespace S100Framework.Applications;

internal static class GeodatabaseFactory
{
    // The caller owns each connection and must dispose it on the ArcGIS thread.
    internal static Func<Geodatabase> Create(string value)
    {
        var address = WorkspaceAddress.Parse(value);
        return address.Kind switch {
            WorkspaceKind.File => () => new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(address.Value))),
            WorkspaceKind.Mobile => () => new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(address.Value))),
            WorkspaceKind.Enterprise => () => new Geodatabase(new DatabaseConnectionFile(new Uri(address.Value))),
            WorkspaceKind.Service => () => new Geodatabase(new ServiceConnectionProperties(new Uri(address.Value))),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }
}
