-- Phase 6 — recording segments. One row per segment file. EndedAt null while
-- the segment is being written; finalized on rotation or stop. SizeBytes is
-- best-effort (filled at finalize) — UI shows '?' for null.
--
-- DROP IF EXISTS first: 001_initial originally seeded a Recordings stub with
-- an INTEGER autoincrement primary key + Path column. Installs that ran the
-- old 001 still carry that stub at migration time, so we drop it before
-- creating the real shape. Stub was empty in practice (no Phase 6 code
-- ever wrote into it), so no data loss.
DROP TABLE IF EXISTS Recordings;
CREATE TABLE Recordings (
    Id          TEXT NOT NULL PRIMARY KEY,
    CameraId    TEXT NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    FilePath    TEXT NOT NULL UNIQUE,
    StartedAt   TEXT NOT NULL,
    EndedAt     TEXT,
    SizeBytes   INTEGER NOT NULL DEFAULT 0,
    Codec       TEXT,
    HasMotion   INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX Idx_Recordings_CameraId_StartedAt ON Recordings(CameraId, StartedAt DESC);
