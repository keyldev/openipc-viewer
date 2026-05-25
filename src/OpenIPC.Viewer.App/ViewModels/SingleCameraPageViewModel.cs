using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.Messages;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Platform;
using OpenIPC.Viewer.Core.Services;
using OpenIPC.Viewer.Core.Video;

namespace OpenIPC.Viewer.App.ViewModels;

public sealed partial class SingleCameraPageViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly IVideoEngine _engine;
    private readonly CameraDirectoryService _directory;
    private readonly IFileSystem _fs;
    private readonly ILogger<SingleCameraPageViewModel> _logger;
    private readonly Camera _camera;

    private IDisposable? _stateSub;
    private IDisposable? _telemetrySub;

    [ObservableProperty] private IVideoSession? _session;
    [ObservableProperty] private SessionState _state = SessionState.Idle;
    [ObservableProperty] private SessionTelemetry? _telemetry;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _snapshotPath;

    public string CameraName => _camera.Name;
    public string HostLabel => _camera.Host;

    public SingleCameraPageViewModel(
        Camera camera,
        IVideoEngine engine,
        CameraDirectoryService directory,
        IFileSystem fs,
        ILogger<SingleCameraPageViewModel> logger)
    {
        _camera = camera;
        _engine = engine;
        _directory = directory;
        _fs = fs;
        _logger = logger;
    }

    public async Task ActivateAsync(CancellationToken ct)
    {
        if (Session is not null)
            return;

        var creds = await _directory.GetCredentialsAsync(_camera.Id, ct).ConfigureAwait(false);
        var options = VideoSessionOptions.Default(_camera.RtspMainUri, creds);
        var session = _engine.CreateSession(options);

        _stateSub = session.StateChanged.Subscribe(s =>
        {
            State = s;
            if (s == SessionState.Failed)
                ErrorMessage = session.LastError;
        });
        _telemetrySub = session.Telemetry.Subscribe(t => Telemetry = t);

        Session = session;
        try
        {
            await session.StartAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start session for camera {CameraId}", _camera.Id);
            ErrorMessage = ex.Message;
            State = SessionState.Failed;
        }
    }

    [RelayCommand]
    private async Task SnapshotAsync()
    {
        if (Session is null)
            return;

        try
        {
            var bytes = await Session.SnapshotAsync(SnapshotFormat.Jpeg, CancellationToken.None).ConfigureAwait(false);
            var dir = Path.Combine(_fs.SnapshotsDir.FullName, SafeFileName(_camera.Name));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd-HHmmss}.jpg");
            await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
            SnapshotPath = path;
            _logger.LogInformation("Snapshot saved {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Snapshot failed");
            ErrorMessage = $"Snapshot failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Back() =>
        WeakReferenceMessenger.Default.Send(new GoBackToLibraryMessage());

    public async ValueTask DisposeAsync()
    {
        _stateSub?.Dispose();
        _telemetrySub?.Dispose();
        if (Session is not null)
        {
            await Session.DisposeAsync().ConfigureAwait(false);
            Session = null;
        }
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0)
                chars[i] = '_';
        }
        return new string(chars);
    }
}
