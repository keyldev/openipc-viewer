using System.Threading;
using Avalonia.Controls;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class ManageGroupsDialog : Window
{
    public ManageGroupsDialog()
    {
        InitializeComponent();
        this.FindControl<Button>("CloseButton")!.Click += (_, _) => Close();
        Opened += async (_, _) =>
        {
            if (DataContext is ManageGroupsViewModel vm)
                await vm.LoadAsync(CancellationToken.None);
        };
    }
}
