using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace OpenIPC.Viewer.Android;

// Avalonia 12 Android entry: AvaloniaAndroidApplication<TApp> is the bridge
// between Android's Application lifecycle and Avalonia's AppBuilder. Without
// this class (and the matching android:name in AndroidManifest), Avalonia
// has no way to know which Application subtype to instantiate — the symptom
// is "Unknown error: AvaloniaView initialization has failed" from
// AvaloniaMainActivity.InitializeAvaloniaView.
[Application]
public sealed class MainApplication : AvaloniaAndroidApplication<App.App>
{
    public MainApplication(System.IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder).WithInterFont();
}
