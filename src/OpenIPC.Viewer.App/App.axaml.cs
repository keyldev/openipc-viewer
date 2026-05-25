using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC.Viewer.App.ViewModels;
using OpenIPC.Viewer.App.Views;

namespace OpenIPC.Viewer.App;

public sealed class App : Application
{
    // Set by the platform host (Desktop's Program.Main or Android's MainActivity)
    // before Avalonia hits OnFrameworkInitializationCompleted. A static slot is
    // the only way to thread IoC across the parameterless ctor that
    // AvaloniaMainActivity<App> requires on Android.
    public static IServiceProvider? Services { get; set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (Services is not null)
        {
            var vm = Services.GetRequiredService<MainWindowViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = vm };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                singleView.MainView = new MainView { DataContext = vm };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
