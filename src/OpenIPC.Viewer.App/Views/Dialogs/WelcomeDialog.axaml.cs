using Avalonia.Controls;
using Avalonia.Threading;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class WelcomeDialog : Window
{
    public WelcomeDialog()
    {
        InitializeComponent();
        var content = this.FindControl<WelcomeDialogContent>("InnerContent")!;
        _ = content.Completion.ContinueWith(t =>
            Dispatcher.UIThread.Post(() => Close(t.Result)),
            System.Threading.Tasks.TaskScheduler.Default);
    }
}
