using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.ViewModels.Dialogs;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Video;

namespace OpenIPC.Viewer.App.Services;

public sealed class CameraEditorFactory
{
    private readonly IVideoEngine _engine;
    private readonly ILoggerFactory _loggerFactory;

    public CameraEditorFactory(IVideoEngine engine, ILoggerFactory loggerFactory)
    {
        _engine = engine;
        _loggerFactory = loggerFactory;
    }

    public CameraEditorViewModel CreateForNew() =>
        new(_engine, _loggerFactory.CreateLogger<CameraEditorViewModel>());

    public CameraEditorViewModel CreateForEdit(Camera existing, CameraCredentials? credentials) =>
        new(existing, credentials, _engine, _loggerFactory.CreateLogger<CameraEditorViewModel>());
}
