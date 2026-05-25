using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using OpenIPC.Viewer.App.ViewModels.Dialogs;
using OpenIPC.Viewer.App.Views.Dialogs;

namespace OpenIPC.Viewer.App.Services;

public sealed class DialogService : IDialogService
{
    public async Task<CameraEditorResult?> ShowCameraEditorAsync(CameraEditorViewModel viewModel)
    {
        var owner = ResolveOwner();
        if (owner is null)
            return null;

        var dlg = new CameraEditorWindow { DataContext = viewModel };
        return await dlg.ShowDialog<CameraEditorResult?>(owner);
    }

    public async Task<DiscoveryDialogResult?> ShowDiscoveryDialogAsync(DiscoveryDialogViewModel viewModel)
    {
        var owner = ResolveOwner();
        if (owner is null)
            return null;

        var dlg = new DiscoveryDialogWindow { DataContext = viewModel };
        return await dlg.ShowDialog<DiscoveryDialogResult?>(owner);
    }

    public async Task<bool> ConfirmAsync(string title, string message, string confirmLabel = "Delete", string cancelLabel = "Cancel")
    {
        var owner = ResolveOwner();
        if (owner is null)
            return false;

        var dlg = new ConfirmDialog();
        dlg.Configure(title, message, confirmLabel, cancelLabel);
        var result = await dlg.ShowDialog<bool?>(owner);
        return result == true;
    }

    public async Task<WelcomeResult> ShowWelcomeAsync()
    {
        var owner = ResolveOwner();
        if (owner is null)
            return WelcomeResult.Skip;

        var dlg = new WelcomeDialog();
        return await dlg.ShowDialog<WelcomeResult>(owner);
    }

    public async Task<string?> PickFolderAsync(string? title = null)
    {
        var owner = ResolveOwner();
        if (owner is null) return null;

        var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title ?? "Pick a folder",
            AllowMultiple = false,
        });
        var first = folders.FirstOrDefault();
        return first?.TryGetLocalPath();
    }

    public async Task ShowManageGroupsAsync(ManageGroupsViewModel viewModel)
    {
        var owner = ResolveOwner();
        if (owner is null) return;
        var dlg = new ManageGroupsDialog { DataContext = viewModel };
        await dlg.ShowDialog(owner);
    }

    private static Window? ResolveOwner() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}
