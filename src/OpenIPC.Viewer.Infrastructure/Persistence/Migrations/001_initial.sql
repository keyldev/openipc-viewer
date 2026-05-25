-- Phase 1 initial schema (architecture §7.1).
-- PtzPresets stays empty for now (no later migration replaces it); the
-- earlier Recordings/Events stubs that lived here were removed because
-- Phase 6/7 redefine those tables with different shapes and the stubs
-- collided at migration time. See 005_recordings.sql / 006_events.sql.

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

CREATE TABLE PtzPresets (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId            TEXT    NOT NULL REFERENCES Cameras(Id) ON DELETE CASCADE,
    OnvifPresetToken    TEXT,
    Name                TEXT    NOT NULL,
    SortOrder           INTEGER NOT NULL DEFAULT 0
);
