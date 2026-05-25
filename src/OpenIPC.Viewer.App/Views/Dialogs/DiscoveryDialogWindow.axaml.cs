using Avalonia.Controls;
using Avalonia.Threading;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class DiscoveryDialogWindow : Window
{
    public DiscoveryDialogWindow()
    {
        InitializeComponent();
        var content = this.FindControl<DiscoveryDialogContent>("InnerContent")!;
        _ = content.Completion.ContinueWith(t =>
            Dispatcher.UIThread.Post(() => Close(t.Result)),
            System.Threading.Tasks.TaskScheduler.Default);
    }
}
