using System.IO;

namespace OpenIPC.Viewer.Core.Platform;

public interface IFileSystem
{
    DirectoryInfo AppDataDir { get; }
    DirectoryInfo RecordingsDir { get; }
    DirectoryInfo SnapshotsDir { get; }
}
