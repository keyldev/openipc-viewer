using System.Threading.Tasks;
using Avalonia.Controls;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class WelcomeDialogContent : UserControl
{
    private readonly TaskCompletionSource<WelcomeResult> _tcs = new();

    public Task<WelcomeResult> Completion => _tcs.Task;

    public WelcomeDialogContent()
    {
        InitializeComponent();
        this.FindControl<Button>("DiscoverButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.Discover);
        this.FindControl<Button>("ScanQrButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.ScanQr);
        this.FindControl<Button>("AddManuallyButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.AddManually);
        this.FindControl<Button>("SkipButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.Skip);
    }
}
