using System.Threading;
using Avalonia.Controls;
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
}
