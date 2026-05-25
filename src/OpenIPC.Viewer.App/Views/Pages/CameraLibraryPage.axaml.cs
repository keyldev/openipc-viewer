using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OpenIPC.Viewer.App.ViewModels;

namespace OpenIPC.Viewer.App.Views.Pages;

public sealed partial class CameraLibraryPage : UserControl
{
    public CameraLibraryPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CameraLibraryPageViewModel vm && !vm.IsLoaded)
            await vm.LoadAsync(CancellationToken.None);
    }

    private void OnCameraCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Handled) return;
        if (sender is not Control { DataContext: CameraRowViewModel row }) return;
        if (DataContext is not CameraLibraryPageViewModel vm) return;
        if (vm.OpenCameraCommand.CanExecute(row))
            vm.OpenCameraCommand.Execute(row);
    }
}
