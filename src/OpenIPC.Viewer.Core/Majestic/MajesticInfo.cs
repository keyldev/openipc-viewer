namespace OpenIPC.Viewer.Core.Majestic;

public sealed record MajesticInfo(
    string? Model,
    string? FirmwareVersion,
    string? ChipModel,
    string? Uptime);
