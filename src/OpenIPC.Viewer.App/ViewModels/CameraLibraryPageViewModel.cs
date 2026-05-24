using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.Services;
using OpenIPC.Viewer.App.ViewModels.Dialogs;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Services;

namespace OpenIPC.Viewer.App.ViewModels;

public sealed partial class CameraLibraryPageViewModel : ViewModelBase
{
    private readonly CameraDirectoryService _directory;
    private readonly IDialogService _dialogs;
    private readonly ILogger<CameraLibraryPageViewModel> _logger;

    public string Title => "Cameras";
    public ObservableCollection<CameraRowViewModel> Cameras { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCameras))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isLoaded;

    public bool HasCameras => IsLoaded && Cameras.Count > 0;
    public bool IsEmpty => IsLoaded && Cameras.Count == 0;

    public CameraLibraryPageViewModel(
        CameraDirectoryService directory,
        IDialogService dialogs,
        ILogger<CameraLibraryPageViewModel> logger)
    {
        _directory = directory;
        _dialogs = dialogs;
        _logger = logger;
        Cameras.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasCameras));
            OnPropertyChanged(nameof(IsEmpty));
        };
    }

    public async Task LoadAsync(CancellationToken ct)
    {
        var cameras = await _directory.ListAsync(ct).ConfigureAwait(true);
        Cameras.Clear();
        foreach (var camera in cameras)
            Cameras.Add(new CameraRowViewModel(camera));
        IsLoaded = true;
    }

    [RelayCommand]
    private async Task AddCameraAsync()
    {
        var editor = new CameraEditorViewModel();
        var result = await _dialogs.ShowCameraEditorAsync(editor).ConfigureAwait(true);
        if (result?.NewRequest is not { } req)
            return;

        try
        {
            await _directory.AddAsync(req, CancellationToken.None).ConfigureAwait(true);
            await LoadAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add camera {Name}", req.Name);
        }
    }

    [RelayCommand]
    private async Task EditCameraAsync(CameraRowViewModel? row)
    {
        if (row is null)
            return;

        var creds = await _directory.GetCredentialsAsync(row.Camera.Id, CancellationToken.None).ConfigureAwait(true);
        var editor = new CameraEditorViewModel(row.Camera, creds);
        var result = await _dialogs.ShowCameraEditorAsync(editor).ConfigureAwait(true);
        if (result?.UpdateRequest is not { } req)
            return;

        try
        {
            await _directory.UpdateAsync(row.Camera.Id, req, CancellationToken.None).ConfigureAwait(true);
            await LoadAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update camera {Id}", row.Camera.Id);
        }
    }

    [RelayCommand]
    private async Task DeleteCameraAsync(CameraRowViewModel? row)
    {
        if (row is null)
            return;

        var confirmed = await _dialogs.ConfirmAsync(
            title: "Delete camera",
            message: $"Delete '{row.Camera.Name}'? This cannot be undone.").ConfigureAwait(true);

        if (!confirmed)
            return;

        try
        {
            await _directory.RemoveAsync(row.Camera.Id, CancellationToken.None).ConfigureAwait(true);
            await LoadAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete camera {Id}", row.Camera.Id);
        }
    }
}

public sealed class CameraRowViewModel
{
    public Camera Camera { get; }
    public string Name => Camera.Name;
    public string HostAndPort => Camera.HttpPort == 80
        ? Camera.Host
        : $"{Camera.Host}:{Camera.HttpPort}";

    public CameraRowViewModel(Camera camera) => Camera = camera;
}
