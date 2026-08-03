using System.Xml.Serialization;

namespace JrEncoder.Schema;

[XmlRoot(ElementName = "Flavor")]
public class Flavor
{
    /// <summary>
    /// Flavor name (usually a letter)
    /// </summary>
    [XmlAttribute(AttributeName = "Name")]
    public required string Name { get; set; }

    /// <summary>
    /// List of pages for the flavor
    /// </summary>
    [XmlElement(ElementName = "Page")]
    public required List<FlavorPage> Page { get; set; }
}