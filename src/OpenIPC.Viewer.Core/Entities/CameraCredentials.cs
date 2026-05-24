namespace OpenIPC.Viewer.Core.Entities;

// In-memory only — never persisted. Stored values live in ISecretsStore via refs.
public sealed record CameraCredentials(string Username, string Password);
