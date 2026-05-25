using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenIPC.Viewer.App.ViewModels;

namespace OpenIPC.Viewer.App.Views.Pages;

public sealed partial class SingleCameraPage : UserControl
{
    public SingleCameraPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SingleCameraPageViewModel vm)
            await vm.ActivateAsync(CancellationToken.None);
    }

    private async void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SingleCameraPageViewModel vm)
            await vm.DisposeAsync();
    }
}
