using System.Text.Json.Serialization;

namespace JrEncoder;

using System.Text.Json;

public class Config
{
    [JsonPropertyName("apikey")]
    public required string APIKey { get; set; }

    [JsonPropertyName("page_interval")]
    public required int PageInterval { get; set; }

    [JsonPropertyName("stars")]
    public required WeatherStar[] Stars { get; set; }

    public class WeatherStar
    {
        [JsonPropertyName("switches")]
        public required string Switches { get; set; }

        [JsonPropertyName("location")]
        public required string Location { get; set; }

        [JsonPropertyName("location_name")]
        public required string LocationName { get; set; }
        
        [JsonPropertyName("nearby_cities")]
        public NearbyLocations? NearbyCities { get; set; }
        
        [JsonPropertyName("regional_cities")]
        public NearbyLocations? RegionalCities { get; set; }

        public double GetLat()
        {
            string[] latLon = Location.Split(',');
            return double.Parse(latLon[0]);
        }

        public double GetLon()
        {
            string[] latLon = Location.Split(',');
            return double.Parse(latLon[1]);
        }
    }

    public class NearbyLocations
    {
        [JsonPropertyName("location_name")]
        public required List<string> Locations { get; set; }

        [JsonPropertyName("geocode")]
        public required List<string> Geocodes { get; set; }
    }

    /// <summary>
    /// Save config file to config.json
    /// </summary>
    public void Save()
    {
        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(GetPath(), JsonSerializer.Serialize(this, options));
    }

    public static string GetPath()
    {
        return Path.Combine(Util.GetExeLocation(), "config.json");
    }

    /// <summary>
    /// Load config from config.json
    /// </summary>
    /// <returns></returns>
    public static Config LoadConfig()
    {
        string configPath = GetPath();
        if (!File.Exists(configPath))
            throw new InvalidOperationException("Config.json does not exist. Run program with --create-config");

        string fileContent = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<Config>(fileContent) ?? throw new InvalidOperationException("Invalid config.json file");
    }

    public static void CreateConfig()
    {
        Config newConfig = new()
        {
            APIKey = "",
            PageInterval = 30,
            Stars =
            [
                new WeatherStar()
                {
                    Location = "Lat,Lon",
                    LocationName = "Location Name",
                    Switches = "00000000",
                }
            ],
        };
        newConfig.Save();
    }
}