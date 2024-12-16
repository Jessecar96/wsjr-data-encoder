namespace JrEncoder;

public class PageAttributes
{
    private bool freeze;
    private bool advisory;
    private bool warning;
    private bool flip;
    private bool roll;
    private bool chain;

    /**
     * Attributes for a page header frame
     *
     * @param freeze   Provides for top display, and blank lower display area, on solid background
     * @param advisory Provides for top display of advisory message text, and bottom display of local instruments, on solid background.
     * @param warning  Provides for top display of warning message text, and bottom display of local instruments, on solid background. Additionally, activates the audio beeper for 10 seconds.
     * @param flip     Display of any chained pages will be accomplished by successive display of one page at a time.
     * @param roll     Display of any chained pages will be accomplished by continuous text roll.
     * @param chain    Indicates the next higher numbered page is logically attached to this page.
     */
    public PageAttributes(bool freeze, bool advisory, bool warning, bool flip, bool roll, bool chain) {
        this.freeze = freeze;
        this.advisory = advisory;
        this.warning = warning;
        this.flip = flip;
        this.roll = roll;
        this.chain = chain;
    }

    /**
     * Defaults, all flags disabled
     */
    public PageAttributes() {
        this.freeze = false;
        this.advisory = false;
        this.warning = false;
        this.flip = false;
        this.roll = false;
        this.chain = false;
    }

    /**
     * Build the two byte value for page attributes
     */
    public byte[] buildAttributes() {
        byte[] attributes = new byte[2];
        attributes[0] = (byte) ((freeze ? 1 : 0) << 2 | (advisory ? 1 : 0) << 1 | (warning ? 1 : 0));
        attributes[1] = (byte) ((flip ? 1 : 0) << 2 | (roll ? 1 : 0) << 1 | (chain ? 1 : 0));
        return attributes;
    }
}