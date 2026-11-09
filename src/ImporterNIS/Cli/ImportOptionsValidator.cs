using S100Framework.ImporterNIS.Core;
using System.Text.RegularExpressions;
using IO = System.IO;

namespace S100Framework.Applications;

internal static class ImportOptionsValidator
{
    internal static void ValidateOptions(ImportOptions options) {
        if (options.Dataset is not null)
            throw new ArgumentException("--dataset is not implemented for NIS imports. Use --filter to select products.");
        var source = WorkspaceAddress.Parse(options.Source!);
        var target = WorkspaceAddress.Parse(options.Target!);
        source.RequireExisting();
        if (source.HasSameAddress(target))
            throw new ArgumentException("Source and target must be different workspaces.");
        if (options.InitializeTarget) {
            if (target.Kind != WorkspaceKind.Mobile)
                throw new ArgumentException("--initialize-target supports new .geodatabase files only.");

            if (File.Exists(target.Value)) {
                var name = IO.Path.GetFileNameWithoutExtension(target.Value);
                foreach (var f in IO.Directory.GetFiles(IO.Path.GetDirectoryName(target.Value)!, $"*{name}*.geodatabase*")) {
                    IO.File.Delete(IO.Path.GetFullPath(f));
                }
            }
            if (Directory.Exists(target.Value)) {
                IO.Directory.Delete(IO.Path.GetFullPath(target.Value), true);
            }
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "s100ed18.geodatabase")))
                throw new FileNotFoundException("The bundled target template is missing.");
            if (!Directory.Exists(Path.GetDirectoryName(target.Value)))
                throw new DirectoryNotFoundException("The target parent directory must exist.");
        }
        else target.RequireExisting();
        if (!string.IsNullOrWhiteSpace(options.S128)) {
            var s128 = WorkspaceAddress.Parse(options.S128);
            if (source.HasSameAddress(s128))
                throw new ArgumentException("Source and S-128 target must be different workspaces.");
            s128.RequireExisting();
        }
        if ((options.MaximumDisplayScale ?? 0) < 0 ||
            (options.MinimumDisplayScale ?? int.MaxValue) <= (options.MaximumDisplayScale ?? 0))
            throw new ArgumentException("Display scales require 0 <= maximumDisplayScale < minimumDisplayScale.");
        if (options.SkinOfEarthOnly is not null && !bool.TryParse(options.SkinOfEarthOnly, out _))
            throw new ArgumentException("--skinofearthonly requires true or false.");
        if (!string.IsNullOrWhiteSpace(options.Filter))
            _ = new Regex(options.Filter, RegexOptions.None, TimeSpan.FromSeconds(2));
        foreach (var directory in new[] { options.NotesPath, options.ScaminFilesPath })
            if (directory is not null && !Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Input directory does not exist: {directory}");
    }

}
