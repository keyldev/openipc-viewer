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
    private readonly IServiceProvider? _services;

    public App() { }

    public App(IServiceProvider services)
    {
        _services = services;
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && _services is not null)
        {
            var vm = _services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
