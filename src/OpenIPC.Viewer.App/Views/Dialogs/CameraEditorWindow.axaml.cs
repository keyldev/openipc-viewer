using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class CameraEditorWindow : Window
{
    public CameraEditorWindow()
    {
        InitializeComponent();

        this.FindControl<Button>("CancelButton")!.Click += (_, _) => Close(null);
        this.FindControl<Button>("SaveButton")!.Click += OnSave;
        Opened += async (_, _) =>
        {
            if (DataContext is CameraEditorViewModel vm)
                await vm.LoadGroupsAsync(CancellationToken.None);
        };
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CameraEditorViewModel vm)
            return;

        if (!vm.TryBuildRequest(out var newRequest, out var updateRequest))
            return;

        Close(new CameraEditorResult(newRequest, updateRequest));
    }
}
