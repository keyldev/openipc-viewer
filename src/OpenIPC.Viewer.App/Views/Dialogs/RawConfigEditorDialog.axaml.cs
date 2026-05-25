using System.Text.Json;
using Avalonia.Controls;
using OpenIPC.Viewer.App.Services;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class RawConfigEditorDialog : Window
{
    public RawConfigEditorDialog()
    {
        InitializeComponent();

        var editor = this.FindControl<TextBox>("Editor")!;
        var apply = this.FindControl<Button>("ApplyButton")!;
        var cancel = this.FindControl<Button>("CancelButton")!;
        var error = this.FindControl<TextBlock>("ErrorBlock")!;

        cancel.Click += (_, _) => Close((string?)null);
        apply.Click += (_, _) =>
        {
            var text = editor.Text ?? "";
            // Sanity-check parses before handing off — caller will also
            // validate at the HTTP layer but failing here keeps the dialog
            // open with an inline error instead of erroring out post-close.
            try
            {
                using var _ = JsonDocument.Parse(text);
            }
            catch (JsonException ex)
            {
                error.Text = string.Format(Localizer.Instance["RawConfigEditor.InvalidJsonFormat"], ex.Message);
                error.IsVisible = true;
                return;
            }
            Close(text);
        };
    }

    public void SetInitialText(string text)
    {
        var editor = this.FindControl<TextBox>("Editor")!;
        editor.Text = text;
    }
}
