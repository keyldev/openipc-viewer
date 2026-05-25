using System.Threading.Tasks;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Services;

public interface IDialogService
{
    Task<CameraEditorResult?> ShowCameraEditorAsync(CameraEditorViewModel viewModel);

    Task<DiscoveryDialogResult?> ShowDiscoveryDialogAsync(DiscoveryDialogViewModel viewModel);

    Task<bool> ConfirmAsync(string title, string message, string confirmLabel = "Delete", string cancelLabel = "Cancel");

    Task<WelcomeResult> ShowWelcomeAsync();

    Task<string?> PickFolderAsync(string? title = null);

    Task ShowManageGroupsAsync(ManageGroupsViewModel viewModel);
}
