using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using OpenIPC.Viewer.App.Services;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class RawConfigEditorContent : UserControl
{
    private readonly TaskCompletionSource<string?> _tcs = new();

    public Task<string?> Completion => _tcs.Task;

    public RawConfigEditorContent()
    {
        InitializeComponent();

        var editor = this.FindControl<TextEditor>("Editor")!;
        editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");
        var apply = this.FindControl<Button>("ApplyButton")!;
        var cancel = this.FindControl<Button>("CancelButton")!;
        var error = this.FindControl<TextBlock>("ErrorBlock")!;

        cancel.Click += (_, _) => _tcs.TrySetResult(null);
        apply.Click += (_, _) =>
        {
            try
            {
                using var _ = JsonDocument.Parse(editor.Text);
            }
            catch (JsonException ex)
            {
                error.Text = string.Format(Localizer.Instance["RawConfigEditor.InvalidJsonFormat"], ex.Message);
                error.IsVisible = true;
                return;
            }
            _tcs.TrySetResult(editor.Text);
        };
    }

    public void SetInitialText(string text)
    {
        var editor = this.FindControl<TextEditor>("Editor")!;
        editor.Text = text;
    }
}
