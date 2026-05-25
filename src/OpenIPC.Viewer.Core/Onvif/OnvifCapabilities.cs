using System;

namespace OpenIPC.Viewer.Core.Onvif;

public sealed record OnvifCapabilities(Uri? MediaServiceUri, Uri? PtzServiceUri)
{
    public bool HasMedia => MediaServiceUri is not null;
    public bool HasPtz => PtzServiceUri is not null;
}
