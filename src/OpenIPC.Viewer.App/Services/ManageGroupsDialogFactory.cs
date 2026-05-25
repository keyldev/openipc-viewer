using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.ViewModels.Dialogs;
using OpenIPC.Viewer.Core.Services;

namespace OpenIPC.Viewer.App.Services;

public sealed class ManageGroupsDialogFactory
{
    private readonly CameraDirectoryService _directory;
    private readonly ILoggerFactory _loggerFactory;

    public ManageGroupsDialogFactory(CameraDirectoryService directory, ILoggerFactory loggerFactory)
    {
        _directory = directory;
        _loggerFactory = loggerFactory;
    }

    public ManageGroupsViewModel Create() =>
        new(_directory, _loggerFactory.CreateLogger<ManageGroupsViewModel>());
}
