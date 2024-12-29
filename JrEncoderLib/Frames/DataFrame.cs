using System;

namespace JrEncoderLib.Frames;

public class DataFrame
{
    protected const byte ClockRunIn = 0x55;
    protected const byte FramingCode = 0x27;

    protected byte[] frame = new byte[38];

    /// <summary>
    /// Return byte array for this frame
    /// </summary>
    /// <returns></returns>
    public virtual byte[] GetBytes()
    {
        return frame;
    }

    /// <summary>
    /// Get this frame as a hex string
    /// </summary>
    /// <returns></returns>
    public string GetFrameAsString()
    {
        return Convert.ToHexString(GetBytes());
    }

    /// <summary>
    /// Convert the specified index range of a byte array to hamming code bytes
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    protected void HamBytes(int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            frame[i] = HammingCode.Get(frame[i]);
        }
    }
}