namespace JrEncoderLib.StarAttributes;

public struct TextLineAttributes
{
    /// <summary>
    /// Adds a solid line above the text when it is rendered.
    /// </summary>
    public bool Separator = false;

    /// <summary>
    /// Makes the text flash.
    /// </summary>
    public bool Flash = false;

    /// <summary>
    /// Switches the color of the text fill and the text border.
    /// </summary>
    public bool Reverse = false;

    /// <summary>
    /// Enables an outline around the text.
    /// </summary>
    public bool Border = true;

    /// <summary>
    /// The color of the text.
    /// </summary>
    public Color Color = Color.Blue;

    /// <summary>
    /// The width of the text. 0 or 1
    /// </summary>
    public int Width = 0;

    /// <summary>
    /// The height of the text. 0, 1, or 2
    /// </summary>
    public int Height = 1;

    public TextLineAttributes()
    {
    }

    /// <summary>
    /// Returns the two-byte packet for this TextLineAttributes.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        byte[] attributes = new byte[2];
        attributes[0] = (byte)((Separator ? 1 : 0) << 3 | (Flash ? 1 : 0) << 2 | (Reverse ? 1 : 0) << 1 | (Border ? 1 : 0));
        attributes[1] = (byte)Color;
        return attributes;
    }

    /// <summary>
    /// Returns the first byte of the packet representation.
    /// </summary>
    /// <returns></returns>
    public byte GetByte1()
    {
        return ToBytes()[0];
    }

    /// <summary>
    /// Returns the second byte of the packet representation.
    /// </summary>
    /// <returns></returns>
    public byte GetByte2()
    {
        return ToBytes()[1];
    }
}