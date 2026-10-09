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
        {
            // No flavors file, copy the example one to the root dir
            File.Copy(Path.Combine(Util.GetExeLocation(), "Examples", "Flavors.xml"), flavorsFilePath);
        }

        // Deserialize XML
        XmlSerializer serializer = new XmlSerializer(typeof(Flavors));
        using StreamReader reader = new StreamReader(flavorsFilePath);
        return (Flavors?)serializer.Deserialize(reader);
    }

    /// <summary>
    /// Save flavors to Flavors.xml
    /// </summary>
    public void Save()
    {
        // Serialize XML
        string flavorsFilePath = GetPath();
        XmlSerializer serializer = new XmlSerializer(typeof(Flavors));
        using StreamWriter writer = new StreamWriter(flavorsFilePath);
        serializer.Serialize(writer, this);
    }
}