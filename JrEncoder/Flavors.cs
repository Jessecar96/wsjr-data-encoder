using System.Xml.Serialization;

namespace JrEncoder;

[XmlRoot(ElementName = "Page")]
public class FlavorPage
{
    /// <summary>
    /// Page name
    /// </summary>
    [XmlAttribute(AttributeName = "Name")]
    public required string Name { get; set; }

    /// <summary>
    /// Page duration in seconds
    /// </summary>
    [XmlAttribute(AttributeName = "Duration")]
    public required int Duration { get; set; }

    /// <summary>
    /// LDL style (optional)
    /// </summary>
    [XmlAttribute(AttributeName = "LDL")]
    public string? LDL { get; set; }
}

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

[XmlRoot(ElementName = "Flavors")]
public class Flavors
{
    [XmlElement(ElementName="Flavor")] 
    public required List<Flavor> Flavor { get; set; } 
}