using System;

namespace OpenIPC.Viewer.Core.Onvif;

public sealed record OnvifProbeResult(
    Uri RtspMainUri,
    string ProfileToken,
    bool HasPtz,
    string? Manufacturer,
    string? Model,
    string? FirmwareVersion);
