using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

    private static Window? ResolveOwner() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}
