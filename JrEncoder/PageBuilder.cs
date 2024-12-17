using JrEncoder.StarAttributes;

namespace JrEncoder;

public class PageBuilder(int pageNumber, Address address, OMCW omcw)
{
    private PageAttributes _attributes;
    private readonly TextLineAttributes[] _lineAttributes = new TextLineAttributes[8];
    private readonly List<TextLineFrame> _textFrames = [];

    /// <summary>
    /// Adds a new line of text to the page.
    /// If the line is the ninth line, the attributes will be dumped and the attributes for the 8th line will be used.
    /// This is due to hardware implementation.
    /// </summary>
    /// <param name="text">The text to show.</param>
    /// <param name="attribs">The attributes of the text.</param>
    /// <returns></returns>
    public PageBuilder AddLine(string text, TextLineAttributes attribs)
    {
        // Calculate bits for text size
        byte textSize = (byte)attribs.Width;
        byte heightBits = (byte)((attribs.Height << 2) & 0x0F);
        textSize = (byte)((textSize | heightBits) & 0x0F);
        
        // Assign attributes & add the frame
        int currentPages = _textFrames.Count;
        if (currentPages <= 8) // There can only be 8 attributes, but up to 9 lines. Attribute 8 is carried to 9.
            _lineAttributes[currentPages] = attribs;

        _textFrames.Add(new TextLineFrame(currentPages + 1, textSize, text));
        return this;
    }

    /// <summary>
    /// Adds a new line of text to the page.
    /// The line will be added with historically-default text attributes (blue color, with border, width 0, height 1).
    /// </summary>
    /// <param name="text">The text to show.</param>
    /// <returns></returns>
    public PageBuilder AddLine(string text)
    {
        return AddLine(text, new TextLineAttributes
        {
            Color = Color.Blue,
            Border = true,
            Width = 0,
            Height = 1
        });
    }

    public PageBuilder Attributes(PageAttributes attributes)
    {
        _attributes = attributes;
        return this;
    }
    
    /// <summary>
    /// Builds the packet for this page to send to the STAR.
    /// </summary>
    /// <returns></returns>
    public DataFrame[] Build()
    {
        int lineCount = _textFrames.Count;
        int frameIndex = 1;
        DataFrame[] frames = new DataFrame[lineCount + 1];

        // Add the header to our output frames
        frames[0] = new PageHeaderFrame(
            pageNumber, lineCount, omcw, address, _attributes, _lineAttributes[0],
            _lineAttributes[1], _lineAttributes[2], _lineAttributes[3], _lineAttributes[4], _lineAttributes[5],
            _lineAttributes[6], _lineAttributes[7]);

        // Add all the text lines
        foreach (TextLineFrame textFrame in _textFrames)
        {
            frames[frameIndex] = textFrame;
            frameIndex++;
        }

        return frames;
    }
}