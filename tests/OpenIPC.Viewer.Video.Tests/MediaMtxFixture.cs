using System.Net.Sockets;

namespace OpenIPC.Viewer.Video.Tests;

// Probes whether the bluenviron/mediamtx container is reachable on the standard
// rtsp port. Tests that need a live stream skip themselves if not.
//
// Usage:
//   docker compose -f tools/mediamtx/docker-compose.yml up -d
//   dotnet test tests/OpenIPC.Viewer.Video.Tests
public static class MediaMtxFixture
{
    public const string TestStreamUri = "rtsp://localhost:8554/test";

    public static bool IsReachable(int timeoutMs = 500)
    {
        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync("localhost", 8554);
            return task.Wait(timeoutMs) && client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
