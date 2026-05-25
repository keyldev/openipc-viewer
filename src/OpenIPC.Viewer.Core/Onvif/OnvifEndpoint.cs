using System;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Onvif;

public sealed record OnvifEndpoint(Uri DeviceServiceUri, CameraCredentials? Credentials)
{
    public static OnvifEndpoint FromHost(string host, int port, CameraCredentials? credentials) =>
        new(new Uri($"http://{host}:{port}/onvif/device_service"), credentials);
}
