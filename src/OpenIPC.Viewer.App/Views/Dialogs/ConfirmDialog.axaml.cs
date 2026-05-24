using Avalonia.Controls;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public void Configure(string title, string message, string confirmLabel, string cancelLabel)
    {
        Title = title;
        this.FindControl<TextBlock>("TitleBlock")!.Text = title;
        this.FindControl<TextBlock>("MessageBlock")!.Text = message;
        var confirm = this.FindControl<Button>("ConfirmButton")!;
        var cancel = this.FindControl<Button>("CancelButton")!;
        confirm.Content = confirmLabel;
        cancel.Content = cancelLabel;
        confirm.Click += (_, _) => Close(true);
        cancel.Click += (_, _) => Close(false);
    }
}
