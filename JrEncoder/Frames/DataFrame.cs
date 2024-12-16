namespace JrEncoder;

public class DataFrame
{
    protected const byte ClockRunIn = 0x55;
    protected const byte FramingCode = 0x27;

    protected byte[] frame = new byte[38];

    public byte[] getFrame() {
        return frame;
    }

    /**
     * @return A string of HEX values for debugging purposes
     */
    public string getFrameAsString()
    {
        return Convert.ToHexString(getFrame());
    }

    /**
     * Convert an index range of the frame to hamming code bytes
     *
     * @param start Start index
     * @param end   Ending index
     */
    public void hamBytes(int start, int end) {
        for (int i = start; i <= end; i++) {
            frame[i] = HammingCode.Get(frame[i]);
        }
    }
}