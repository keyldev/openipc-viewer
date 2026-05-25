using System.Threading.Tasks;
using Avalonia.Controls;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class DiscoveryDialogContent : UserControl
{
    private readonly TaskCompletionSource<DiscoveryDialogResult?> _tcs = new();

    public Task<DiscoveryDialogResult?> Completion => _tcs.Task;

    public DiscoveryDialogContent()
    {
        InitializeComponent();

        this.FindControl<Button>("CancelButton")!.Click += (_, _) =>
        {
            if (DataContext is DiscoveryDialogViewModel vm) vm.Cancel();
            _tcs.TrySetResult(null);
        };

        this.FindControl<Button>("AddButton")!.Click += async (_, _) =>
        {
            if (DataContext is not DiscoveryDialogViewModel vm) return;
            var result = await vm.AddSelectedAsync();
            if (result is not null)
                _tcs.TrySetResult(result);
        };
    }
}
