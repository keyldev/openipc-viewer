using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Platform;

namespace OpenIPC.Viewer.Infrastructure.Secrets;

[SupportedOSPlatform("windows")]
public sealed class DpapiSecretsStore : ISecretsStore
{
    private const int SaltLength = 32;

    private readonly string _storePath;
    private readonly string _saltPath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DpapiSecretsStore(DirectoryInfo appDataDir)
    {
        _storePath = Path.Combine(appDataDir.FullName, "secrets.bin");
        _saltPath = Path.Combine(appDataDir.FullName, "secrets.salt");
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var store = await LoadAsync(ct).ConfigureAwait(false);
            if (!store.TryGetValue(key, out var ciphertext))
                return null;

            var salt = await LoadOrCreateSaltAsync(ct).ConfigureAwait(false);
            var plaintext = ProtectedData.Unprotect(
                Convert.FromBase64String(ciphertext), salt, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintext);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetAsync(string key, string value, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var store = await LoadAsync(ct).ConfigureAwait(false);
            var salt = await LoadOrCreateSaltAsync(ct).ConfigureAwait(false);
            var ciphertext = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(value), salt, DataProtectionScope.CurrentUser);
            store[key] = Convert.ToBase64String(ciphertext);
            await SaveAsync(store, ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var store = await LoadAsync(ct).ConfigureAwait(false);
            if (store.Remove(key))
                await SaveAsync(store, ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Dictionary<string, string>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_storePath))
            return new Dictionary<string, string>(StringComparer.Ordinal);

        await using var stream = File.OpenRead(_storePath);
        var data = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, cancellationToken: ct)
            .ConfigureAwait(false);
        return data ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private async Task SaveAsync(Dictionary<string, string> store, CancellationToken ct)
    {
        var tempPath = _storePath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, store, cancellationToken: ct).ConfigureAwait(false);
        }
        File.Move(tempPath, _storePath, overwrite: true);
    }

    private async Task<byte[]> LoadOrCreateSaltAsync(CancellationToken ct)
    {
        if (File.Exists(_saltPath))
            return await File.ReadAllBytesAsync(_saltPath, ct).ConfigureAwait(false);

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var tempPath = _saltPath + ".tmp";
        await File.WriteAllBytesAsync(tempPath, salt, ct).ConfigureAwait(false);
        File.Move(tempPath, _saltPath, overwrite: true);
        return salt;
    }
}
