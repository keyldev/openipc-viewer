using System;
using System.Threading;
using Android.App;
using Android.Runtime;
using Android.Util;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC.Viewer.Core.Events;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Android;

// Avalonia 12 Android entry: AvaloniaAndroidApplication<TApp> is the bridge
// between Android's Application lifecycle and Avalonia's AppBuilder. The DI
// container must be built HERE (not in MainActivity.OnCreate) because
// AvaloniaAndroidApplication.OnCreate triggers App.OnFrameworkInitializationCompleted
// during base.OnCreate — by the time MainActivity exists, Avalonia has
// already decided whether to set MainView. App.Services must be populated
// before base.OnCreate runs or the user sees a blank screen.
[Application]
public sealed class MainApplication : AvaloniaAndroidApplication<App.App>
{
    private const string Tag = "OpenIPC";

    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        try
        {
            var services = Composition.Build(this);
            App.App.Services = services;

            services.GetRequiredService<IMigrationRunner>()
                .MigrateAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

            services.GetRequiredService<EventIngestionService>()
                .StartAsync(CancellationToken.None)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Error(Tag, Java.Lang.Throwable.FromException(ex), "Composition.Build failed");
            throw;
        }

        base.OnCreate();
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder).WithInterFont();
}
