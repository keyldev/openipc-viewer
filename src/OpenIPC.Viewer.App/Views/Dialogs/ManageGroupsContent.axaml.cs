using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class ManageGroupsContent : UserControl
{
    // Manage-groups dialog has no per-action result — caller awaits "user
    // dismissed". CloseButton flips the TCS; the inner row commands mutate
    // the VM in-place against the live repository.
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task<bool> Completion => _tcs.Task;

    public ManageGroupsContent()
    {
        InitializeComponent();
        this.FindControl<Button>("CloseButton")!.Click += (_, _) => _tcs.TrySetResult(true);
        AttachedToVisualTree += async (_, _) =>
        {
            if (DataContext is ManageGroupsViewModel vm)
                await vm.LoadAsync(CancellationToken.None);
        };
    }
}
