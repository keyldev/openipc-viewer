-- Phase 7 — camera events (motion, connection, snapshot, etc.).
-- OccurredAt + EndedAt store UTC ISO-8601; UI converts to local.
-- Source is the IMotionEventSource.Name that emitted the tick (manual /
-- majestic / onvif / syslog) so we can later filter by detection method.
--
-- DROP IF EXISTS first: same story as 005_recordings — 001_initial used to
-- ship a stub Events table with different columns (Type/Payload). Installs
-- that ran the old 001 carry it until this migration; stub was empty.
DROP TABLE IF EXISTS Events;
CREATE TABLE Events (
    Id          TEXT NOT NULL PRIMARY KEY,
    CameraId    TEXT NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    Kind        INTEGER NOT NULL,
    Severity    INTEGER NOT NULL,
    OccurredAt  TEXT NOT NULL,
    EndedAt     TEXT,
    Source      TEXT,
    Summary     TEXT
);
CREATE INDEX Idx_Events_Camera_Occurred ON Events(CameraId, OccurredAt DESC);
CREATE INDEX Idx_Events_Kind_Occurred ON Events(Kind, OccurredAt DESC);
