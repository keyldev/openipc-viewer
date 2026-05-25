# OpenIPC Viewer

Cross-platform desktop and mobile viewer for OpenIPC IP cameras.
Built with .NET 9 and Avalonia 12.

> Status: **Phase 2** — single-camera RTSP viewer with FFmpeg software decode, snapshots, and SQLite-backed camera library.

## Build and run

Requires .NET 9 SDK + PowerShell (for the FFmpeg fetch script) + Docker (only for integration tests).

```bash
# one-time: pull FFmpeg shared-build DLLs into runtimes/win-x64/native/
powershell -ExecutionPolicy Bypass -File tools/fetch-ffmpeg.ps1

dotnet restore OpenIPC.Viewer.slnx
dotnet build   OpenIPC.Viewer.slnx
dotnet test    OpenIPC.Viewer.slnx --no-build
dotnet run --project src/OpenIPC.Viewer.Desktop
```

Build runs with `TreatWarningsAsErrors=true`; any warning fails the build.

## Native dependencies

FFmpeg shared-build DLLs (LGPL, version pin `n7.1`) are **not** committed.
`tools/fetch-ffmpeg.ps1` downloads them from `BtbN/FFmpeg-Builds` releases and
drops them into `runtimes/win-x64/native/`, which the Video project copies into
the Desktop output. Re-run the script when bumping the FFmpeg pin.

## Layout

```
src/
  OpenIPC.Viewer.Core/            netstandard2.1 — domain, no IO, no UI
  OpenIPC.Viewer.Infrastructure/  net9.0         — SQLite, secrets, config
  OpenIPC.Viewer.Video/           net9.0         — FFmpeg pipeline (FFmpeg.AutoGen + SkiaSharp)
  OpenIPC.Viewer.Devices/         net9.0         — ONVIF, Majestic HTTP
  OpenIPC.Viewer.App/             net9.0         — Avalonia views and viewmodels
  OpenIPC.Viewer.Desktop/         net9.0         — Win/Lin/Mac host, DI composition
tests/
  OpenIPC.Viewer.Core.Tests/      xUnit
  OpenIPC.Viewer.Video.Tests/     xUnit + MediaMTX integration
```

`App` references `Core` only. Infrastructure, Video, Devices are wired into `Desktop` via DI.

## Test fixture: MediaMTX

The video integration test and manual smoke depend on a local RTSP source. A
MediaMTX container is provided that synthesises a 1280×720@25 h264 test pattern
on demand at `rtsp://localhost:8554/test`.

```bash
docker compose -f tools/mediamtx/docker-compose.yml up -d
# ... do your testing ...
docker compose -f tools/mediamtx/docker-compose.yml down
```

The integration test auto-skips itself if the container isn't reachable.

## User data

```
%LOCALAPPDATA%/OpenIPC.Viewer/
  logs/openipc-viewer-{date}.log    rolling daily, 7-day retention
  appsettings.json                  optional override over the one shipped with the app
  openipc-viewer.db                 SQLite database (cameras, recordings metadata, events)
  secrets.bin / secrets.salt        DPAPI-encrypted camera credentials
  snapshots/{camera}/*.jpg          manual snapshots
  recordings/                       reserved for Phase 6
```

## License

MIT. Bundled FFmpeg DLLs are LGPL — they ship as side-by-side shared libs and
can be replaced by the user.
