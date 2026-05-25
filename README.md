# OpenIPC Viewer

Cross-platform desktop and mobile viewer for OpenIPC IP cameras.
Built with .NET 9 / 10 and Avalonia 12.

[![build](https://github.com/keyldev/openipc-viewer/actions/workflows/build.yml/badge.svg)](https://github.com/keyldev/openipc-viewer/actions/workflows/build.yml)

> Status: **Phase 11a** — settings UI, first-run onboarding, brand polish.
> Targets: Windows / Linux / macOS desktops + Android + iOS. First public
> `v0.1.0-beta` release pending.

## Layout

```
src/
  OpenIPC.Viewer.Core/            netstandard2.1 — domain, no IO, no UI, no package deps
  OpenIPC.Viewer.Infrastructure/  net9.0         — SQLite, secrets, decoder factories
  OpenIPC.Viewer.Video/           net9.0         — FFmpeg pipeline (FFmpeg.AutoGen + SkiaSharp)
  OpenIPC.Viewer.Devices/         net9.0         — ONVIF, Majestic HTTP
  OpenIPC.Viewer.App/             net9.0         — Avalonia views and viewmodels (cross-platform)
  OpenIPC.Viewer.Composition/     net9.0         — shared DI registrations (used by every head)
  OpenIPC.Viewer.Desktop/         net9.0         — Win/Lin/Mac host, classic-window lifetime
  OpenIPC.Viewer.Android/         net10.0-android — Android host (min API 31), foreground-service recording
  OpenIPC.Viewer.iOS/             net10.0-ios     — iOS host (min 16), foreground-only recording
tests/
  OpenIPC.Viewer.Core.Tests/      xUnit
  OpenIPC.Viewer.Video.Tests/     xUnit + MediaMTX integration
```

`App` references `Core` only. Infrastructure, Video, Devices and the platform
trio (`IFileSystem` / `ISecretsStore` / `IHwDecoderFactory`) are wired into
each head via `OpenIPC.Viewer.Composition.SharedComposition.AddSharedServices()`.

## Desktop — Windows / Linux / macOS

```bash
dotnet restore OpenIPC.Viewer.slnx
dotnet build   OpenIPC.Viewer.slnx
dotnet test    OpenIPC.Viewer.slnx --no-build
dotnet run --project src/OpenIPC.Viewer.Desktop
```

Build runs with `TreatWarningsAsErrors=true`; any warning fails the build.

**Windows** ships FFmpeg shared-build DLLs side-by-side. Run
`tools/fetch-ffmpeg.ps1` once to download them from `BtbN/FFmpeg-Builds`
(`n7.1` ABI) into `runtimes/win-x64/native/`. Re-run after bumping the pin.

**Linux** uses the system FFmpeg via the default loader path:
```
sudo apt install ffmpeg libavcodec-extra libsecret-1-0 libsecret-tools
```
Credentials read/write through `secret-tool` against GNOME/KDE keyring; an
AES-GCM file fallback (machine-id-derived key) kicks in if D-Bus isn't
available. VAAPI hardware decode needs `/dev/dri/renderD128` and your user
in the `render` (or `video`) group.

**macOS** uses Homebrew FFmpeg:
```
brew install ffmpeg
```
Credentials live in the macOS Keychain via the built-in `security` tool.
VideoToolbox HW decode works on any Mac 12+. Gatekeeper blocks unsigned
downloaded builds on first launch — right-click the .app, choose *Open*
(signing/notarization arrives in a later phase).

## Android

```bash
dotnet workload install android
dotnet build src/OpenIPC.Viewer.Android/OpenIPC.Viewer.Android.csproj -c Release
```

CI cross-compiles FFmpeg `n7.1` for `android-arm64` via NDK r27c on every
build (cached when version/script unchanged). Recording uses in-process
libavformat + a foreground service for OS keep-alive (declared with
`foregroundServiceType=dataSync`); recording on Android requires
`POST_NOTIFICATIONS` runtime permission on Android 13+, prompted on the
first record. Credentials use an AES-GCM file keyed off
`Settings.Secure.AndroidId` — `EncryptedSharedPreferences` via AndroidX
Security lands in a follow-up.

## iOS

```bash
dotnet workload install ios
dotnet build src/OpenIPC.Viewer.iOS/OpenIPC.Viewer.iOS.csproj -c Release    # Mac-only for the link step
```

iOS recording is **foreground-only** — Apple doesn't grant 24/7 background
captures to surveillance apps. Credentials use an AES-GCM file keyed off
`UIDevice.identifierForVendor`; real Keychain via `Security.framework`
P/Invoke is a follow-up. CI builds an unsigned `.app`/`.ipa`; TestFlight
signing arrives in the release-polish phase.

> **CI-only validation caveat.** Linux/macOS/Android/iOS code paths are
> built + linked in CI but not yet end-to-end tested on real devices for
> every commit. Feedback from desktop-Linux, Mac, Android and iOS users
> is very welcome — open an issue with the OS version, what you did,
> what happened.

## Test fixture: MediaMTX

The video integration test and manual smoke depend on a local RTSP source.
A MediaMTX container is provided that synthesises a 1280×720@25 h264 test
pattern on demand at `rtsp://localhost:8554/test`.

```bash
docker compose -f tools/mediamtx/docker-compose.yml up -d
# ... do your testing ...
docker compose -f tools/mediamtx/docker-compose.yml down
```

The integration test auto-skips itself if the container isn't reachable.

## User data

Per-platform AppData root:

| OS | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\OpenIPC.Viewer\` |
| Linux | `$XDG_DATA_HOME/openipc-viewer/` (default `~/.local/share/openipc-viewer/`) |
| macOS | `~/Library/Application Support/OpenIPC.Viewer/` |
| Android | `/data/data/org.openipc.viewer/files/` (app-private; uninstall wipes) |
| iOS | `~/Library/Application Support/OpenIPC.Viewer/` (sandbox; Files-app visible via UIFileSharingEnabled) |

Inside the root:

```
logs/openipc-viewer-{date}.log    rolling daily, 7-day retention
appsettings.json                  optional override over the one shipped with the app
usersettings.json                 settings written by the in-app Settings page
openipc-viewer.db                 SQLite (cameras, recordings metadata, events)
secrets.bin / secrets.salt        encrypted credential fallback (used when no native keystore)
snapshots/{camera}/*.jpg          manual snapshots
recordings/                       MP4 segments (Linux/macOS may override via XDG_VIDEOS_DIR / ~/Movies)
```

Credentials live in the native keystore when available (Windows DPAPI /
macOS Keychain / Linux libsecret). The encrypted-file fallback is used
when no keystore is reachable.

## License

MIT. Bundled FFmpeg DLLs and self-built FFmpeg `.so` are LGPL — shipped as
side-by-side shared libs, replaceable by the user.
