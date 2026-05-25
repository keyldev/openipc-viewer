namespace OpenIPC.Viewer.Core.Onvif;

// All axes normalized to [-1.0, 1.0]. ONVIF spec PTZ space defaults to this range;
// some cameras use a different scale but most cooperate. See phase-04 risks for
// PanTiltLimits normalization that we skip in MVP.
public readonly record struct PtzVelocity(float PanX, float TiltY, float Zoom)
{
    public static readonly PtzVelocity Zero = default;
}
