using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using OpenIPC.Viewer.App.Services;
using OpenIPC.Viewer.App.ViewModels.Dialogs;

namespace OpenIPC.Viewer.App.Views.Dialogs;

public sealed partial class WelcomeDialogContent : UserControl
{
    private readonly TaskCompletionSource<WelcomeResult> _tcs = new();
    private UserSettingsService? _settings;

    public Task<WelcomeResult> Completion => _tcs.Task;

    public WelcomeDialogContent()
    {
        InitializeComponent();
        this.FindControl<Button>("DiscoverButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.Discover);
        this.FindControl<Button>("ScanQrButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.ScanQr);
        this.FindControl<Button>("AddManuallyButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.AddManually);
        this.FindControl<Button>("SkipButton")!.Click += (_, _) => _tcs.TrySetResult(WelcomeResult.Skip);
    }

    // Wired by DialogService before the dialog is shown. Hydrates the language
    // ComboBox from the persisted setting and writes back through UserSettingsService
    // on change — the Settings.Changed event in Composition.Apply re-routes that
    // into Localizer.SetLanguage so all bindings flip live without relaunch.
    public void Configure(UserSettingsService settings)
    {
        _settings = settings;
        var picker = this.FindControl<ComboBox>("LanguagePicker")!;
        picker.SelectedItem = settings.Current.Language;
        picker.SelectionChanged += (_, _) =>
        {
            if (picker.SelectedItem is not string code || code == settings.Current.Language)
                return;
            _ = settings.UpdateAsync(settings.Current with { Language = code }, CancellationToken.None);
        };
    }
}
