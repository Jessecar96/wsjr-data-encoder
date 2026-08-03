using System.Xml.Serialization;

namespace JrEncoder.Schema;

[XmlRoot(ElementName = "Flavors")]
public class Flavors
{
    [XmlElement(ElementName = "Flavor")]
    public required List<Flavor> Flavor { get; set; }

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