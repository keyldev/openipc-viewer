using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace OpenIPC.Viewer.Android;

[Activity(
    Label = "OpenIPC Viewer",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@mipmap/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize
                          | ConfigChanges.UiMode | ConfigChanges.Density)]
public sealed class MainActivity : AvaloniaMainActivity
{
    // All composition + early-startup work runs in MainApplication.OnCreate
    // so App.Services is populated before Avalonia's OnFrameworkInitializationCompleted
    // fires. This activity is the launcher entry; AvaloniaMainActivity does
    // the rest (window + view setup).
}
