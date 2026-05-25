using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC.Viewer.Core.Events;
using OpenIPC.Viewer.Core.Persistence;

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
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Build composition + run startup before Avalonia kicks off — same
        // ordering as Desktop/Program.cs (services ready before the first view).
        var services = Composition.Build(ApplicationContext!);
        App.App.Services = services;

        services.GetRequiredService<IMigrationRunner>()
            .MigrateAsync(CancellationToken.None)
            .GetAwaiter().GetResult();

        services.GetRequiredService<EventIngestionService>()
            .StartAsync(CancellationToken.None)
            .GetAwaiter().GetResult();

        base.OnCreate(savedInstanceState);
    }
}
