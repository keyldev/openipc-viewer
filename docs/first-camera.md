# Adding your first camera

Three paths, listed cheapest-to-most-manual.

## 1. WS-Discovery (LAN scan)

Library → `🔍 Discover`. The app sends a WS-Discovery probe to the
multicast group `239.255.255.250:3702`. Cameras with ONVIF responders
answer within ~5 s; the dialog lists them with their advertised model and
RTSP URI.

Pick a camera → enter credentials → the editor pre-fills name / host /
RTSP / ONVIF profile. Save.

If discovery turns up nothing:
- Your router is blocking multicast (check WiFi AP settings — some block
  it on guest networks by default).
- The camera is on a different VLAN.
- ONVIF is disabled in the camera config (Majestic ships with it disabled
  in some firmware revisions — flip `service.onvif.enabled = true`).

## 2. Manual

Library → `+ Add camera`. Fill in:

- **Host** — IP or hostname.
- **RTSP main** — full URI, e.g. `rtsp://192.168.1.50:554/0`. Path varies
  by firmware: OpenIPC mainline uses `/0` (main) and `/1` (sub).
- **Credentials** — kept in the platform keystore (DPAPI / Keychain /
  libsecret) or AES-GCM file fallback.

## 3. QR code

*Comes in 11c.* Future: scan a QR with `rtsp://user:pass@host:port/path`
or a JSON payload, app adds the camera in one tap.

## Common issues

- **"Failed to connect" / `stimeout` errors** — wrong RTSP path. Camera
  vendors love to pick different defaults (`/cam/realmonitor`, `/stream1`,
  `/h264`). Check the vendor docs.
- **Auth loops** — the URI takes plain user/password but credentials live
  in the keystore separately. Setting them in both places is fine; just
  the keystore copy is preferred (URI auth shows up in logs).
- **VAAPI permission denied** (Linux) — `usermod -aG render $USER` then
  re-login. The app falls back to software decode but eats more CPU.
- **Android: missing notification permission** — recording shows a
  notification while it runs. On Android 13+ the runtime permission
  prompt fires the first time you start a recording.
