using System.Xml.Serialization;

namespace JrEncoder.Schema;

[XmlRoot(ElementName = "Page")]
public class CustomPage
{
    /// <summary>
    /// List of each line in the page
    /// </summary>
    [XmlElement(ElementName = "Line")]
    public List<PageLine> Line = [];
}