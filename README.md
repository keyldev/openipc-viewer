# OpenIPC Viewer

Cross-platform desktop and mobile viewer for OpenIPC IP cameras.
Built with .NET 9 and Avalonia 12.

> Status: **Phase 0** — empty shell. No camera functionality yet.

## Build and run

Requires .NET 9 SDK.

```bash
dotnet restore OpenIPC.Viewer.slnx
dotnet build   OpenIPC.Viewer.slnx
dotnet test    OpenIPC.Viewer.slnx --no-build
dotnet run --project src/OpenIPC.Viewer.Desktop
```

Build runs with `TreatWarningsAsErrors=true`; any warning fails the build.

## Layout

```
src/
  OpenIPC.Viewer.Core/            netstandard2.1 — domain, no IO, no UI
  OpenIPC.Viewer.Infrastructure/  net9.0         — SQLite, secrets, config
  OpenIPC.Viewer.Video/           net9.0         — FFmpeg pipeline
  OpenIPC.Viewer.Devices/         net9.0         — ONVIF, Majestic HTTP
  OpenIPC.Viewer.App/             net9.0         — Avalonia views and viewmodels
  OpenIPC.Viewer.Desktop/         net9.0         — Win/Lin/Mac host, DI composition
tests/
  OpenIPC.Viewer.Core.Tests/      xUnit
```

`App` references `Core` only. Infrastructure, Video, Devices are wired into `Desktop` via DI.

## User data

```
%LOCALAPPDATA%/OpenIPC.Viewer/
  logs/openipc-viewer-{date}.log    rolling daily, 7-day retention
  appsettings.json                  optional override over the one shipped with the app
```

## License

MIT.
