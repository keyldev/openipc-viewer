# Getting started

The [README](../README.md) has per-platform install instructions for all five
targets — Windows / Linux / macOS / Android / iOS. This file expands on a few
points that don't fit there.

## Where things land

| Thing | Location |
|---|---|
| App settings UI writes | `usersettings.json` in your platform's AppData root |
| Camera DB | `openipc-viewer.db` (SQLite) in the same root |
| Recordings | `recordings/` (or `~/Movies/OpenIPC` on Linux/macOS if `XDG_VIDEOS_DIR` is set) |
| Snapshots | `snapshots/` (or `~/Pictures/OpenIPC` on Linux/macOS) |
| Logs | `logs/openipc-viewer-{date}.log` (rolling daily, 7-day retention) |

Per-platform AppData root paths are in the README's **User data** section.

## First-run flow

On first launch the library is empty and a welcome dialog appears with three
choices:

1. **Scan local network** — runs WS-Discovery against your subnet (Phase 4).
   Most cameras with a working ONVIF responder are picked up within ~5 s.
2. **Add manually** — host + RTSP path form. Use this if discovery misses the
   camera or if it's on a different VLAN.
3. **Skip** — opens an empty library; add cameras later from `+ Add camera`.

The "show this once" flag is persisted; dismissing it once means the dialog
doesn't reappear even if you later delete every camera.

## Tuning

- **Settings → Video → Show telemetry overlay** — toggles the live-view
  badges (codec / fps / frames). Off by default for a cleaner picture during
  recordings.
- **Settings → Video → Default RTSP transport** — `tcp` is the safer default
  on lossy networks. Switch to `udp` only if you've ruled out packet loss.
- **Settings → Advanced → Verbose logging** — flips Serilog's minimum level
  to Debug live, no restart needed. Useful when something doesn't connect
  and you want to see why before pinging an issue.
