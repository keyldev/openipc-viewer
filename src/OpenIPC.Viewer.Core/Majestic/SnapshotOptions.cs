namespace OpenIPC.Viewer.Core.Majestic;

// Width/Height null = native sensor resolution. Majestic resizes server-side
// on /image.jpg, which is what makes it cheap enough for grid thumbnails.
public sealed record MajesticSnapshotOptions(int? Width = null, int? Height = null);
