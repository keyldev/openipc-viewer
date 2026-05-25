namespace OpenIPC.Viewer.App.Services;

// User-tweakable preferences persisted to AppDataDir/usersettings.json.
// Distinct from appsettings.json — appsettings is shipped defaults +
// optional override, this is "what the user toggled in the Settings page".
// Default values match the current hard-coded behavior so an upgrade with
// a missing settings file behaves identically.
public sealed record UserSettings(
    bool ShowTelemetryOverlay = true,
    bool VerboseLogging = false,
    bool AutoScanLanOnStartup = false,
    int MaxConcurrentGridSessions = 9,
    string RtspTransport = "tcp",
    bool WelcomeShown = false)
{
    public static UserSettings Default => new();
}
