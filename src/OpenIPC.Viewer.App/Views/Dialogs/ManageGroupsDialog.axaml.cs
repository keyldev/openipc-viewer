using Avalonia.Controls;
using Avalonia.Threading;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class ManageGroupsDialog : Window
{
    public ManageGroupsDialog()
    {
        InitializeComponent();
        var content = this.FindControl<ManageGroupsContent>("InnerContent")!;
        _ = content.Completion.ContinueWith(_ =>
            Dispatcher.UIThread.Post(() => Close()),
            System.Threading.Tasks.TaskScheduler.Default);
    }
}
