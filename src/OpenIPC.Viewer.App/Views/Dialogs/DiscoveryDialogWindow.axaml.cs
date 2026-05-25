using Avalonia.Controls;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class DiscoveryDialogWindow : Window
{
    public DiscoveryDialogWindow()
    {
        InitializeComponent();

        this.FindControl<Button>("CancelButton")!.Click += (_, _) =>
        {
            if (DataContext is DiscoveryDialogViewModel vm) vm.Cancel();
            Close(null);
        };

        this.FindControl<Button>("AddButton")!.Click += async (_, _) =>
        {
            if (DataContext is not DiscoveryDialogViewModel vm) return;
            var result = await vm.AddSelectedAsync();
            if (result is not null)
                Close(result);
        };
    }
}
