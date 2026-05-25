using System.IO;
using System.Runtime.Versioning;
using System.Text;
using Android.Content;
using Android.Provider;
using OpenIPC.Viewer.Core.Platform;
using OpenIPC.Viewer.Infrastructure.Secrets;

namespace OpenIPC.Viewer.Android.Platform;

// Phase 9a uses the AES-GCM file fallback (same code path as headless Linux),
// keyed off Settings.Secure.AndroidId — stable per device + app signature,
// guaranteed available since API 1. Phase 9b will swap in
// EncryptedSharedPreferences via AndroidX.Security for keystore-backed
// protection, but it needs an extra binding package that 9a doesn't take on.
[SupportedOSPlatform("android")]
public sealed class AndroidSecretsStore : ISecretsStore
{
    private readonly EncryptedFileSecretsStore _inner;

    public AndroidSecretsStore(Context context, DirectoryInfo appDataDir)
    {
        var androidId = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId)
                        ?? "openipc-viewer-fallback";
        var keyMaterial = Encoding.UTF8.GetBytes(androidId);
        _inner = new EncryptedFileSecretsStore(appDataDir, keyMaterial);
    }

    public System.Threading.Tasks.Task<string?> GetAsync(string key, System.Threading.CancellationToken ct)
        => _inner.GetAsync(key, ct);

    public System.Threading.Tasks.Task SetAsync(string key, string value, System.Threading.CancellationToken ct)
        => _inner.SetAsync(key, value, ct);

    public System.Threading.Tasks.Task RemoveAsync(string key, System.Threading.CancellationToken ct)
        => _inner.RemoveAsync(key, ct);
}
