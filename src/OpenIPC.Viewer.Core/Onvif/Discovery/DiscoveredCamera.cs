using System;

namespace OpenIPC.Viewer.Core.Onvif.Discovery;

public sealed record DiscoveredCamera(
    string Host,
    int OnvifPort,
    Uri DeviceServiceUri,
    string? Name,
    string? Model);
