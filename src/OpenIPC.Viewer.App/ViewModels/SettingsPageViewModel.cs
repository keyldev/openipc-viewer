using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC.Viewer.App.Services;
using OpenIPC.Viewer.Core.Platform;

namespace OpenIPC.Viewer.App.ViewModels;

public sealed partial class SettingsPageViewModel : ViewModelBase
{
    private readonly UserSettingsService _settings;
    private readonly IFileSystem _fs;
    private readonly IDialogService _dialogs;
    private bool _suppressSave;

    public string Title => "Settings";

    [ObservableProperty] private bool _showTelemetryOverlay;
    [ObservableProperty] private bool _verboseLogging;
    [ObservableProperty] private bool _autoScanLanOnStartup;
    [ObservableProperty] private int _maxConcurrentGridSessions;
    [ObservableProperty] private string _rtspTransport = "tcp";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveRecordingsDirectory))]
    [NotifyPropertyChangedFor(nameof(IsRecordingsDirOverridden))]
    private string _recordingsDirOverride = "";

    public bool IsRecordingsDirOverridden => !string.IsNullOrWhiteSpace(RecordingsDirOverride);

    // What RecordingService will actually use — override if set, otherwise
    // the platform default. Updated reactively via the two NotifyPropertyChangedFor.
    public string EffectiveRecordingsDirectory =>
        IsRecordingsDirOverridden ? RecordingsDirOverride : _fs.RecordingsDir.FullName;

    public int[] GridSessionOptions { get; } = new[] { 4, 9, 16, 25 };
    public string[] TransportOptions { get; } = new[] { "tcp", "udp" };

    public string AppDataDirectory => _fs.AppDataDir.FullName;
    public string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.1.0";
    public string RepositoryUrl => "https://github.com/keyldev/openipc-viewer";

    public SettingsPageViewModel(UserSettingsService settings, IFileSystem fs, IDialogService dialogs)
    {
        _settings = settings;
        _fs = fs;
        _dialogs = dialogs;
        Load();
    }

    private void Load()
    {
        // _suppressSave keeps the OnXxxChanged hooks from re-saving while we
        // hydrate from disk (each property setter would otherwise round-trip
        // the file once on construction).
        _suppressSave = true;
        try
        {
            var s = _settings.Current;
            ShowTelemetryOverlay = s.ShowTelemetryOverlay;
            VerboseLogging = s.VerboseLogging;
            AutoScanLanOnStartup = s.AutoScanLanOnStartup;
            MaxConcurrentGridSessions = s.MaxConcurrentGridSessions;
            RtspTransport = s.RtspTransport;
            RecordingsDirOverride = s.RecordingsDirOverride;
        }
        finally { _suppressSave = false; }
    }

    partial void OnShowTelemetryOverlayChanged(bool value) => Persist();
    partial void OnVerboseLoggingChanged(bool value) => Persist();
    partial void OnAutoScanLanOnStartupChanged(bool value) => Persist();
    partial void OnMaxConcurrentGridSessionsChanged(int value) => Persist();
    partial void OnRtspTransportChanged(string value) => Persist();
    partial void OnRecordingsDirOverrideChanged(string value) => Persist();

    private void Persist()
    {
        if (_suppressSave) return;
        var next = _settings.Current with
        {
            ShowTelemetryOverlay = ShowTelemetryOverlay,
            VerboseLogging = VerboseLogging,
            AutoScanLanOnStartup = AutoScanLanOnStartup,
            MaxConcurrentGridSessions = MaxConcurrentGridSessions,
            RtspTransport = RtspTransport,
            RecordingsDirOverride = RecordingsDirOverride,
        };
        // Fire-and-forget; binding setters are synchronous and any save
        // error is logged inside UpdateAsync.
        _ = _settings.UpdateAsync(next, CancellationToken.None);
    }

    [RelayCommand]
    private async Task PickRecordingsDirectoryAsync()
    {
        var picked = await _dialogs.PickFolderAsync("Pick recordings folder").ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(picked))
            RecordingsDirOverride = picked;
    }

    [RelayCommand]
    private void ResetRecordingsDirectory() => RecordingsDirOverride = "";

    [RelayCommand]
    private void OpenAppDataDirectory() => OpenInShell(_fs.AppDataDir.FullName);

    [RelayCommand]
    private void OpenRecordingsDirectory() => OpenInShell(EffectiveRecordingsDirectory);

    [RelayCommand]
    private void OpenRepository() => OpenInShell(RepositoryUrl);

    private static void OpenInShell(string target)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true,
            });
        }
        catch (Exception) { /* best effort — Android sandbox blocks shell-open, desktop works */ }
    }
}
