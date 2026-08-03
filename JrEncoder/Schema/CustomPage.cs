using System.Xml.Serialization;

namespace JrEncoder.Schema;

[XmlRoot(ElementName = "Page")]
public class CustomPage
{
    /// <summary>
    /// Page name
    /// </summary>
    [XmlAttribute(AttributeName = "Name")]
    public required string Name { get; set; }

    /// <summary>
    /// List of each line in the page
    /// </summary>
    [XmlElement(ElementName = "Line")]
    public List<PageLine> Line = [];
}