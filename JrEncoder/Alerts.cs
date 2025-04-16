using System.Xml.Serialization;

namespace JrEncoder;

[XmlRoot(ElementName = "Alert")]
public class Alert
{
    /// <summary>
    /// Event name
    /// </summary>
    [XmlAttribute(AttributeName = "Name")]
    public required string Name { get; set; }

    /// <summary>
    /// If this alert will show as a headline
    /// </summary>
    [XmlAttribute(AttributeName = "Headline")]
    public bool Headline { get; set; } = false;

    /// <summary>
    /// If this alert will show as a scroll
    /// </summary>
    [XmlAttribute(AttributeName = "Scroll")]
    public bool Scroll { get; set; } = false;

    /// <summary>
    /// If scroll is enabled, the severity of the scroll (Warning or Advisory)
    /// </summary>
    [XmlAttribute(AttributeName = "Severity")]
    public WarningType Severity { get; set; } = WarningType.Advisory;
}

[XmlRoot(ElementName = "Alerts")]
public class Alerts
{
    [XmlElement(ElementName = "Alert")]
    public required List<Alert> Alert { get; set; }
}