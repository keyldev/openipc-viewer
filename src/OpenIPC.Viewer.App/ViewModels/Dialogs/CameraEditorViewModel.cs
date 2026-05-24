using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.App.ViewModels.Dialogs;

public sealed partial class CameraEditorViewModel : ViewModelBase
{
    public CameraId? EditingId { get; }
    public string Title => EditingId is null ? "Add camera" : "Edit camera";

    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _host = "";
    [ObservableProperty] private int _httpPort = 80;
    [ObservableProperty] private string _onvifPortText = "";
    [ObservableProperty] private string _rtspMainText = "";
    [ObservableProperty] private string _rtspSubText = "";
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";

    [ObservableProperty] private string? _errorMessage;

    public bool CanTestConnection => false; // wired in Phase 2

    public CameraEditorViewModel() { }

    public CameraEditorViewModel(Camera existing, CameraCredentials? credentials)
    {
        EditingId = existing.Id;
        Name = existing.Name;
        Host = existing.Host;
        HttpPort = existing.HttpPort;
        OnvifPortText = existing.OnvifPort?.ToString() ?? "";
        RtspMainText = existing.RtspMainUri.ToString();
        RtspSubText = existing.RtspSubUri?.ToString() ?? "";
        Username = credentials?.Username ?? "";
        Password = credentials?.Password ?? "";
    }

    [RelayCommand]
    private void AutoDeriveRtsp()
    {
        if (!string.IsNullOrWhiteSpace(Host))
            RtspMainText = $"rtsp://{Host.Trim()}/";
    }

    public bool TryBuildRequest(out NewCameraRequest? newRequest, out UpdateCameraRequest? updateRequest)
    {
        newRequest = null;
        updateRequest = null;
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            ErrorMessage = "Host is required.";
            return false;
        }

        var rtspMainSource = string.IsNullOrWhiteSpace(RtspMainText)
            ? $"rtsp://{Host.Trim()}/"
            : RtspMainText.Trim();

        if (!Uri.TryCreate(rtspMainSource, UriKind.Absolute, out var rtspMain))
        {
            ErrorMessage = "RTSP main URI is not a valid absolute URI.";
            return false;
        }

        Uri? rtspSub = null;
        if (!string.IsNullOrWhiteSpace(RtspSubText))
        {
            if (!Uri.TryCreate(RtspSubText.Trim(), UriKind.Absolute, out rtspSub))
            {
                ErrorMessage = "RTSP sub URI is not a valid absolute URI.";
                return false;
            }
        }

        int? onvifPort = null;
        if (!string.IsNullOrWhiteSpace(OnvifPortText))
        {
            if (!int.TryParse(OnvifPortText.Trim(), out var port) || port < 1 || port > 65535)
            {
                ErrorMessage = "ONVIF port must be between 1 and 65535.";
                return false;
            }
            onvifPort = port;
        }

        if (HttpPort < 1 || HttpPort > 65535)
        {
            ErrorMessage = "HTTP port must be between 1 and 65535.";
            return false;
        }

        var credentials = string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)
            ? null
            : new CameraCredentials(Username, Password);

        if (EditingId is null)
        {
            newRequest = new NewCameraRequest(
                Name: Name.Trim(),
                Host: Host.Trim(),
                HttpPort: HttpPort,
                OnvifPort: onvifPort,
                RtspMainUri: rtspMain,
                RtspSubUri: rtspSub,
                Credentials: credentials);
        }
        else
        {
            updateRequest = new UpdateCameraRequest(
                Name: Name.Trim(),
                Host: Host.Trim(),
                HttpPort: HttpPort,
                OnvifPort: onvifPort,
                RtspMainUri: rtspMain,
                RtspSubUri: rtspSub,
                Credentials: credentials);
        }

        return true;
    }
}

public sealed record CameraEditorResult(NewCameraRequest? NewRequest, UpdateCameraRequest? UpdateRequest);
