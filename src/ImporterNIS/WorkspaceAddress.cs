namespace S100Framework.ImporterNIS.Core;

public enum WorkspaceKind { File, Mobile, Enterprise, Service }

public sealed record WorkspaceAddress(WorkspaceKind Kind, string Value)
{
    public static WorkspaceAddress Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile) {
            if (uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) ||
                !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                throw new ArgumentException("Service addresses must use HTTPS without credentials, queries or fragments.", nameof(value));
            return new(WorkspaceKind.Service, uri.AbsoluteUri.TrimEnd('/'));
        }

        string path = Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var kind = Path.GetExtension(path).ToLowerInvariant() switch {
            ".gdb" => WorkspaceKind.File,
            ".geodatabase" => WorkspaceKind.Mobile,
            ".sde" => WorkspaceKind.Enterprise,
            _ => throw new ArgumentException("Expected a .gdb, .geodatabase, .sde or HTTPS service address.", nameof(value))
        };
        return new(kind, path);
    }

    public bool HasSameAddress(WorkspaceAddress other) =>
        Kind == other.Kind && string.Equals(Value, other.Value,
            Kind == WorkspaceKind.Service ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

    public void RequireExisting()
    {
        if (Kind == WorkspaceKind.Service) return;
        if (Kind == WorkspaceKind.File ? !Directory.Exists(Value) : !System.IO.File.Exists(Value))
            throw new FileNotFoundException("Workspace does not exist.", Value);
    }
}
