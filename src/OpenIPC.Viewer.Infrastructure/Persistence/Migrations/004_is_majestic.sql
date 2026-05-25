-- Phase 5 — flag that gates the Majestic Config panel in SingleCameraPage.
-- Set to 1 after a successful IMajesticClient.PingAsync. Existing rows stay
-- at 0 until the next page open; the page auto-probes on first visit.
ALTER TABLE Cameras ADD COLUMN IsMajestic INTEGER NOT NULL DEFAULT 0;
