namespace OpenIPC.Viewer.Core.Video;

public interface IVideoEngine
{
    IVideoSession CreateSession(VideoSessionOptions options);
}
