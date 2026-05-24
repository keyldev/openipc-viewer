using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OpenIPC.Viewer.App.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public LivePageViewModel Live { get; }
    public CameraLibraryPageViewModel Library { get; }
    public RecordingsPageViewModel Recordings { get; }
    public SettingsPageViewModel Settings { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLiveSelected))]
    [NotifyPropertyChangedFor(nameof(IsLibrarySelected))]
    [NotifyPropertyChangedFor(nameof(IsRecordingsSelected))]
    [NotifyPropertyChangedFor(nameof(IsSettingsSelected))]
    private ViewModelBase _currentPage;

    public bool IsLiveSelected => CurrentPage is LivePageViewModel;
    public bool IsLibrarySelected => CurrentPage is CameraLibraryPageViewModel;
    public bool IsRecordingsSelected => CurrentPage is RecordingsPageViewModel;
    public bool IsSettingsSelected => CurrentPage is SettingsPageViewModel;

    public MainWindowViewModel(
        LivePageViewModel live,
        CameraLibraryPageViewModel library,
        RecordingsPageViewModel recordings,
        SettingsPageViewModel settings)
    {
        Live = live;
        Library = library;
        Recordings = recordings;
        Settings = settings;
        _currentPage = library; // Phase 1 lands on Library so the new page is visible by default.
    }

    [RelayCommand]
    private void Navigate(string target)
    {
        CurrentPage = target switch
        {
            "live" => Live,
            "library" => Library,
            "recordings" => Recordings,
            "settings" => Settings,
            _ => CurrentPage,
        };
    }
}
