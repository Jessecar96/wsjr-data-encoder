namespace JrEncoder;

public class TextLineAttributes
{
    private bool separator;
    private bool flash;
    private bool reverse;
    private bool border;
    private int color;

    /**
     * Attributes for a text line frame
     *
     * @param separator Enable a solid line above the line of text
     * @param flash     Flash the line of text
     * @param reverse   Reverse the color of the text
     * @param border    Enable an outline of drop-shadow on the text (default)
     * @param color     Text color
     */
    public TextLineAttributes(bool separator, bool flash, bool reverse, bool border, Color color) {
        this.separator = separator;
        this.flash = flash;
        this.reverse = reverse;
        this.border = border;
        this.color = (int)color;
    }

    /**
     * Defaults that were used in most cases
     */
    public TextLineAttributes() {
        this.separator = false;
        this.flash = false;
        this.reverse = false;
        this.border = true;
        this.color = 5;
    }

    /**
     * Build the two byte value for text line attributes
     */
    public byte[] buildAttributes() {
        byte[] attributes = new byte[2];
        attributes[0] = (byte) ((separator ? 1 : 0) << 3 | (flash ? 1 : 0) << 2 | (reverse ? 1 : 0) << 1 | (border ? 1 : 0));
        attributes[1] = (byte) color;
        return attributes;
    }

    public byte getByte1() {
        return this.buildAttributes()[0];
    }

    public byte getByte2() {
        return this.buildAttributes()[1];
    }
    
    byte setBit(byte data, int pos, int val)
    {
        return (byte) ((val << pos) | data );
    }
}