using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.App.ViewModels;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Platform;
using OpenIPC.Viewer.Core.Services;
using OpenIPC.Viewer.Core.Video;

namespace OpenIPC.Viewer.App.Services;

public sealed class SingleCameraPageFactory
{
    private readonly IVideoEngine _engine;
    private readonly CameraDirectoryService _directory;
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;

    public SingleCameraPageFactory(
        IVideoEngine engine,
        CameraDirectoryService directory,
        IFileSystem fs,
        ILoggerFactory loggerFactory)
    {
        _engine = engine;
        _directory = directory;
        _fs = fs;
        _loggerFactory = loggerFactory;
    }

    public SingleCameraPageViewModel Create(Camera camera) =>
        new(camera, _engine, _directory, _fs, _loggerFactory.CreateLogger<SingleCameraPageViewModel>());
}
