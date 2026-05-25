using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.ViewModels.Dialogs;
using OpenIPC.Viewer.Core.Onvif;
using OpenIPC.Viewer.Core.Onvif.Discovery;

namespace OpenIPC.Viewer.App.Services;

public sealed class DiscoveryDialogFactory
{
    private readonly IDiscoveryService _discovery;
    private readonly OnvifProbeService _probe;
    private readonly ILoggerFactory _loggerFactory;

    public DiscoveryDialogFactory(
        IDiscoveryService discovery,
        OnvifProbeService probe,
        ILoggerFactory loggerFactory)
    {
        _discovery = discovery;
        _probe = probe;
        _loggerFactory = loggerFactory;
    }

    public DiscoveryDialogViewModel Create() =>
        new(_discovery, _probe, _loggerFactory.CreateLogger<DiscoveryDialogViewModel>());
}
