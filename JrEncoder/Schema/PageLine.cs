using System.Xml.Serialization;
using JrEncoderLib.StarAttributes;

namespace JrEncoder.Schema;

[XmlRoot(ElementName = "Line")]
public class PageLine
{
    /// <summary>
    /// Enable line separator, see <see cref="TextLineAttributes.Separator"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Separator")]
    public bool Separator = false;

    /// <summary>
    /// Enable line flashing, see <see cref="TextLineAttributes.Flash"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Flash")]
    public bool Flash = false;

    /// <summary>
    /// Enable reverse text color, see <see cref="TextLineAttributes.Reverse"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Flash")]
    public bool Reverse = false;

    /// <summary>
    /// Enable character borders, see <see cref="TextLineAttributes.Border"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Border")]
    public bool Border = true;

    /// <summary>
    /// Line color, see <see cref="TextLineAttributes.Color"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Color")]
    public Color Color = Color.Blue;

    /// <summary>
    /// Text width, see <see cref="TextLineAttributes.Width"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Width")]
    public int Width = 0;

    /// <summary>
    /// Text height, see <see cref="TextLineAttributes.Height"/>
    /// </summary>
    [XmlAttribute(AttributeName = "Height")]
    public int Height = 1;

    /// <summary>
    /// Content of the line
    /// </summary>
    [XmlText]
    public required string Text;
}