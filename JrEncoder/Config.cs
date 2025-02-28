using System.Text.Json.Serialization;
using GeoTimeZone;

namespace JrEncoder;

using System.Text.Json;

public class Config
{
    /// <summary>
    /// weather.com API key
    /// </summary>
    [JsonPropertyName("apikey")]
    public required string APIKey { get; set; }

    /// <summary>
    /// Make all time zones be the same
    /// </summary>
    [JsonPropertyName("force_clock_set")]
    public bool ForceClockSet { get; set; } = false;

    /// <summary>
    /// Loop a flavor, if desired
    /// </summary>
    [JsonPropertyName("loop_flavor")]
    public string? LoopFlavor { get; set; } = "L";

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
        public NearbyLocations? NearbyCities { get; set; } = new NearbyLocations
        {
            Locations = [],
            Geocodes = []
        };

        [JsonPropertyName("regional_cities")]
        public NearbyLocations? RegionalCities { get; set; } = new NearbyLocations
        {
            Locations = [],
            Geocodes = []
        };

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

        /// <summary>
        /// Returns an IANA time zone identifier (such as "Europe/London")
        /// </summary>
        /// <returns></returns>
        public string GetTimeZoneIdentifier()
        {
            return TimeZoneLookup.GetTimeZone(GetLat(), GetLon()).Result;
        }

        public TimeZoneInfo GetTimeZoneInfo()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(GetTimeZoneIdentifier());
            }
            catch (Exception ex)
            {
                // Failed to find time zone, use the local system time zone
                Console.WriteLine("Failed to find time zone: " + ex.Message);
                return TimeZoneInfo.Local;
            }
        }
    }

    public class NearbyLocations
    {
        [JsonPropertyName("location_name")]
        public required List<string> Locations { get; set; }

        [JsonPropertyName("geocode")]
        public required List<string> Geocodes { get; set; }

        /// <summary>
        /// Get location name limited by length
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="limit">Length to limit string to</param>
        /// <returns></returns>
        public string GetLocationName(int i, int limit = 14)
        {
            return Locations[i].Length > limit ? Locations[i].Substring(0, limit) : Locations[i];
        }
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
            LoopFlavor = "L",
            ForceClockSet = false,
            Stars =
            [
                new WeatherStar()
                {
                    Location = "Lat,Lon",
                    LocationName = "Location Name",
                    Switches = "00000000",
                }
            ]
        };
        newConfig.Save();
    }
}