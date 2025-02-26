namespace JrEncoderLib.StarAttributes;

public struct PageAttributes
{
    /// <summary>
    /// Forces a transition into "Freeze" mode.
    /// </summary>
    public bool Freeze;

    /// <summary>
    /// Forces a transition into "Advisory" mode.
    /// </summary>
    public bool Advisory;

    /// <summary>
    /// Forces a transition into "Warning" mode.
    /// </summary>
    public bool Warning;

    /// <summary>
    /// The display of "chained" pages will be accomplished by successive display of one page at a time.
    /// <seealso cref="Chain"/>
    /// </summary>
    public bool Flip;

    /// <summary>
    /// The display of "chained" pages will be accomplished by scrolling the display.
    /// <seealso cref="Chain"/>
    /// </summary>
    public bool Roll;

    /// <summary>
    /// Indicates that the next higher-numbered page is logically attached to this page.
    /// This is only used for "Flip" and "Roll" attributes on other pages..
    /// <seealso cref="Flip"/>
    /// <seealso cref="Roll"/>
    /// </summary>
    public bool Chain;

    /// <summary>
    /// Mark this page as a warning crawl - when the star receives it the page will immediately crawl on the LDL for 2 minutes
    /// </summary>
    public bool WarningCrawl;

    /// <summary>
    /// Unused bit, not sure what it does
    /// </summary>
    public bool Unused2;

    /// <summary>
    /// Returns the two-byte packet for this PageAttributes.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        byte[] attributes = new byte[2];
        attributes[0] = (byte)((WarningCrawl ? 1 : 0) << 3 | (Freeze ? 1 : 0) << 2 | (Advisory ? 1 : 0) << 1 | (Warning ? 1 : 0));
        attributes[1] = (byte)((Unused2 ? 1 : 0) << 3 | (Flip ? 1 : 0) << 2 | (Roll ? 1 : 0) << 1 | (Chain ? 1 : 0));
        return attributes;
    }
}