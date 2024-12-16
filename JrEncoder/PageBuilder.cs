namespace JrEncoder;

public class PageBuilder
{
    private int pageNumber;
    private OMCW omcw;
    private Address address;
    private PageAttributes attributes;
    private TextLineAttributes[] lineAttributes = new TextLineAttributes[8];
    private List<TextLineFrame> textFrames; // Max of 9 lines per page

    public PageBuilder(int pageNumber)
    {
        this.pageNumber = pageNumber;
        attributes = new PageAttributes();
        address = new Address();
        textFrames = new List<TextLineFrame>();

        //Init textlineattributes in case we have null lines ...
        for (int i = 0; i < 7; i++)
        {
            lineAttributes[i] = new TextLineAttributes();
        }
    }

    public PageBuilder setLineAttributes(TextLineAttributes lineAttributes1, TextLineAttributes lineAttributes2,
        TextLineAttributes lineAttributes3, TextLineAttributes lineAttributes4,
        TextLineAttributes lineAttributes5, TextLineAttributes lineAttributes6,
        TextLineAttributes lineAttributes7, TextLineAttributes lineAttributes8)
    {
        this.lineAttributes[0] = lineAttributes1;
        this.lineAttributes[1] = lineAttributes2;
        this.lineAttributes[2] = lineAttributes3;
        this.lineAttributes[3] = lineAttributes4;
        this.lineAttributes[4] = lineAttributes5;
        this.lineAttributes[5] = lineAttributes6;
        this.lineAttributes[6] = lineAttributes7;
        this.lineAttributes[7] = lineAttributes8;
        return this;
    }

    public PageBuilder setPageAttributes(PageAttributes attributes)
    {
        this.attributes = attributes;
        return this;
    }

    /**
     * Set the OMCW for this page
     */
    public PageBuilder setOMCW(OMCW omcw)
    {
        this.omcw = omcw;
        return this;
    }

    /**
     * Set the unit address for this page
     */
    public PageBuilder setAddress(Address address)
    {
        this.address = address;
        return this;
    }

    /**
     * Add a text line with the default attributes, and color
     *
     * @param lineNumber Line Number
     * @param text       Text
     * @param height     Line text height
     * @param width      Line text width
     * @return this
     */
    public PageBuilder addLine(int lineNumber, String text, int height = 1, int width = 0)
    {
        byte textSize = (byte)width;
        byte heightBits = (byte)((height << 2) & 0x0F);
        textSize = (byte)((textSize | heightBits) & 0x0F);
        textFrames.Add(new TextLineFrame(lineNumber, textSize, text));
        return this;
    }

    /**
     * Build the array of frames needed for this page
     *
     * @return Array of frames to send to the STAR
     */
    public DataFrame[] build()
    {
        int lineCount = textFrames.Count;
        int frameIndex = 1;
        DataFrame[] frames = new DataFrame[lineCount + 1];

        // Add the header to our output frames
        frames[0] = new PageHeaderFrame(pageNumber, lineCount, omcw, address, attributes, lineAttributes[0],
            lineAttributes[1], lineAttributes[2], lineAttributes[3], lineAttributes[4], lineAttributes[5],
            lineAttributes[6], lineAttributes[7]);

        // Add all of the text lines
        foreach (TextLineFrame textFrame in textFrames)
        {
            frames[frameIndex] = textFrame;
            frameIndex++;
        }

        return frames;
    }
}