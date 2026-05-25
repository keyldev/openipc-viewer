using System.IO;
using System.Runtime.Versioning;
using Android.Content;
using OpenIPC.Viewer.Core.Platform;

namespace OpenIPC.Viewer.Android.Platform;

// Scoped-storage paths only — no WRITE_EXTERNAL_STORAGE needed on Android 10+.
//   AppDataDir     → /data/data/{packageName}/files (context.FilesDir)
//   RecordingsDir  → /sdcard/Android/data/{packageName}/files/Movies
//   SnapshotsDir   → /sdcard/Android/data/{packageName}/files/Pictures
// All three are app-private; uninstall wipes them.
[SupportedOSPlatform("android")]
public sealed class AndroidFileSystem : IFileSystem
{
    public DirectoryInfo AppDataDir { get; }
    public DirectoryInfo RecordingsDir { get; }
    public DirectoryInfo SnapshotsDir { get; }

    public AndroidFileSystem(Context context)
    {
        AppDataDir = Ensure(context.FilesDir!.AbsolutePath);
        RecordingsDir = Ensure(context.GetExternalFilesDir(global::Android.OS.Environment.DirectoryMovies)?.AbsolutePath
                               ?? Path.Combine(AppDataDir.FullName, "recordings"));
        SnapshotsDir = Ensure(context.GetExternalFilesDir(global::Android.OS.Environment.DirectoryPictures)?.AbsolutePath
                              ?? Path.Combine(AppDataDir.FullName, "snapshots"));
    }

    private static DirectoryInfo Ensure(string path)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists) dir.Create();
        return dir;
    }
}
