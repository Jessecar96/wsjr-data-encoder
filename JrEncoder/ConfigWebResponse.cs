using System.Text.Json.Serialization;

namespace JrEncoder;

public class ConfigWebResponse
{
    [JsonPropertyName("config")]
    public required Config config { get; set; }
    
    [JsonPropertyName("flavors")]
    public required Flavors flavors { get; set; }
}