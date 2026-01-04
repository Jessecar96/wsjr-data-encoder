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

    /// <summary>
    /// If the top section of the screen should have a solid background
    /// </summary>
    [XmlAttribute(AttributeName = "TopSolid")]
    public bool TopSolid { get; set; } = true;

    /// <summary>
    /// If the bottom section of the screen should have a solid background
    /// </summary>
    [XmlAttribute(AttributeName = "BottomSolid")]
    public bool BottomSolid { get; set; } = true;

    /// <summary>
    /// If the line between the two sections of the screen should be shown
    /// </summary>
    [XmlAttribute(AttributeName = "RegionSeparator")]
    public bool RegionSeparator { get; set; } = true;

    /// <summary>
    /// If the radar relay should be enabled
    /// </summary>
    [XmlAttribute(AttributeName = "Radar")]
    public bool Radar { get; set; } = false;

    /// <summary>
    /// If the aux audio relay should be enabled
    /// </summary>
    [XmlAttribute(AttributeName = "AuxAudio")]
    public bool AuxAudio { get; set; } = false;

    /// <summary>
    /// If the local pre-roll relay should be enabled
    /// </summary>
    [XmlAttribute(AttributeName = "LocalPreroll")]
    public bool LocalPreroll { get; set; } = false;

    /// <summary>
    /// If the local program relay should be enabled
    /// </summary>
    [XmlAttribute(AttributeName = "LocalProgram")]
    public bool LocalProgram { get; set; } = false;
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
    [XmlElement(ElementName = "Flavor")] public required List<Flavor> Flavor { get; set; }

    public static string GetPath(string fileName = "Flavors.xml")
    {
        return Path.Combine(Util.GetExeLocation(), fileName);
    }

    /// <summary>
    /// Load flavors from Flavors.xml
    /// </summary>
    /// <returns></returns>
    public static Flavors? LoadFlavors(string fileName = "Flavors.xml")
    {
        string flavorsFilePath = GetPath(fileName);
        if (!File.Exists(flavorsFilePath))
            throw new InvalidOperationException(fileName + " does not exist.");

        XmlSerializer serializer = new(typeof(Flavors));
        using StreamReader reader = new(flavorsFilePath);
        return (Flavors?)serializer.Deserialize(reader);
    }

    /// <summary>
    /// Save flavors to Flavors.xml
    /// </summary>
    public void Save()
    {
        string flavorsFilePath = GetPath();
        XmlSerializer serializer = new(typeof(Flavors));
        using StreamWriter writer = new(flavorsFilePath);
        serializer.Serialize(writer, this);
    }
}