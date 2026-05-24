using System.IO;
using OpenIPC.Viewer.App;
using OpenIPC.Viewer.Core.Platform;

namespace OpenIPC.Viewer.Desktop;

public sealed class DesktopFileSystem : IFileSystem
{
    public DirectoryInfo AppDataDir { get; } = AppPaths.AppDataDir;
    public DirectoryInfo RecordingsDir { get; } = EnsureDir(Path.Combine(AppPaths.AppDataDir.FullName, "recordings"));
    public DirectoryInfo SnapshotsDir { get; } = EnsureDir(Path.Combine(AppPaths.AppDataDir.FullName, "snapshots"));

    private static DirectoryInfo EnsureDir(string path)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists)
            dir.Create();
        return dir;
    }
}
