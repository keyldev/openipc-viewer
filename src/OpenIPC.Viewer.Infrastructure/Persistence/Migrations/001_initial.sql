-- Phase 1 initial schema (architecture §7.1).
-- Recordings / Events / PtzPresets tables are created empty now to avoid
-- a later migration when their owning phases land (see phase-01 §1.2).

CREATE TABLE Groups (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL UNIQUE,
    SortOrder   INTEGER NOT NULL DEFAULT 0,
    CreatedAt   TEXT    NOT NULL
);

CREATE TABLE Cameras (
    Id                  TEXT    PRIMARY KEY,
    GroupId             INTEGER REFERENCES Groups(Id),
    Name                TEXT    NOT NULL,
    Host                TEXT    NOT NULL,
    OnvifPort           INTEGER,
    HttpPort            INTEGER NOT NULL DEFAULT 80,
    RtspMainUri         TEXT    NOT NULL,
    RtspSubUri          TEXT,
    UsernameRef         TEXT,
    PasswordRef         TEXT,
    OnvifEnabled        INTEGER NOT NULL DEFAULT 0,
    OnvifProfileToken   TEXT,
    ChipModel           TEXT,
    FirmwareVersion     TEXT,
    SortOrder           INTEGER NOT NULL DEFAULT 0,
    CreatedAt           TEXT    NOT NULL,
    UpdatedAt           TEXT    NOT NULL
);

CREATE TABLE Recordings (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId    TEXT    NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    Path        TEXT    NOT NULL,
    StartedAt   TEXT    NOT NULL,
    EndedAt     TEXT,
    SizeBytes   INTEGER,
    Codec       TEXT,
    HasMotion   INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX idx_recordings_camera_started ON Recordings(CameraId, StartedAt);

CREATE TABLE Events (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId        TEXT    NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    Type            TEXT    NOT NULL,
    OccurredAt      TEXT    NOT NULL,
    Severity        TEXT    NOT NULL DEFAULT 'Info',
    ThumbnailPath   TEXT,
    Payload         TEXT
);
CREATE INDEX idx_events_camera_occurred ON Events(CameraId, OccurredAt);

CREATE TABLE PtzPresets (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId            TEXT    NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    OnvifPresetToken    TEXT,
    Name                TEXT    NOT NULL,
    SortOrder           INTEGER NOT NULL DEFAULT 0
);
