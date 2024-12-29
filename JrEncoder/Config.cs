using System.Text.Json.Serialization;

namespace JrEncoder;

using System.Text.Json;

public class Config
{
    [JsonPropertyName("apikey")]
    public string APIKey { get; set; }
    
    [JsonPropertyName("stars")]
    public WeatherStar[] Stars { get; set; }

    public class WeatherStar
    {
        [JsonPropertyName("switches")]
        public string Switches { get; set; }
        
        [JsonPropertyName("location")]
        public string Location { get; set; }
        
        [JsonPropertyName("location_name")]
        public string LocationName { get; set; }
    }

    /// <summary>
    /// Save config file to config.json
    /// </summary>
    public void Save()
    {
        Console.WriteLine("Saved config");
        File.WriteAllText(Path.Combine(Util.GetExeLocation(), "config.json"), JsonSerializer.Serialize(this));
    }

    /// <summary>
    /// Load config from config.json
    /// </summary>
    /// <returns></returns>
    public static Config LoadConfig()
    {
        string configPath = Path.Combine(Util.GetExeLocation(), "config.json");
        return LoadConfig(configPath);
    }

    /// <summary>
    /// Load config from custom file name
    /// </summary>
    /// <param name="configPath">File path & name to load config from</param>
    /// <returns></returns>
    public static Config LoadConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            Console.WriteLine("config.json file doesn't exist. Creating a new one.");
            Config newConfig = new();
            newConfig.Save();
            return newConfig;
        }

        string fileContent = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<Config>(fileContent);
    }
}