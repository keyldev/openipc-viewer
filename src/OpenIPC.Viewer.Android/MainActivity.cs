using System;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
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
    private const string Tag = "OpenIPC";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Composition wrapped separately — if DI fails, we want a clear
        // logcat line, not Android's misleading "did not call through to
        // super.onCreate()" message. base.OnCreate stays OUTSIDE the try
        // so Avalonia bootstrap failures bubble untouched (a swallowed
        // exception there breaks Activity.OnCreate's super-call contract).
        try
        {
            var services = Composition.Build(ApplicationContext!);
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

        base.OnCreate(savedInstanceState);
    }
}
