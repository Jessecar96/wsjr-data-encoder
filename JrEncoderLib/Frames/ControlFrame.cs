using JrEncoderLib.StarAttributes;

namespace JrEncoderLib.Frames;

public class ControlFrame : DataFrame
{
    protected OMCW omcw;

    /// <summary>
    /// Set the OMCW bytes in the frame's byte array
    /// </summary>
    private void SetOmcwBytes()
    {
        // OMCW is always at bytes 4-7
        byte[] omcwBytes = omcw.ToBytes();
        frame[4] = omcwBytes[0];
        frame[5] = omcwBytes[1];
        frame[6] = omcwBytes[2];
        frame[7] = omcwBytes[3];

        // OMCW bytes must be hammed after being modified
        HamBytes(4, 7);
    }

    /// <summary>
    /// Return byte array for this frame
    /// </summary>
    /// <returns></returns>
    public override byte[] GetBytes()
    {
        // Set the OMCW bytes every time we get the frame bytes so we always have an up to date OMCW
        SetOmcwBytes();
        return base.GetBytes();
    }
}