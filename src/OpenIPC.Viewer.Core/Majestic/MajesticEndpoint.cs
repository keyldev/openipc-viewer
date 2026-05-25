using System;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Majestic;

public sealed record MajesticEndpoint(string Host, int HttpPort, CameraCredentials? Credentials)
{
    public Uri BaseUri => HttpPort == 80
        ? new Uri($"http://{Host}/")
        : new Uri($"http://{Host}:{HttpPort}/");
}
