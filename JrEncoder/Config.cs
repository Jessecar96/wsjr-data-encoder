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
    public string? LoopFlavor { get; set; } = null;

    [JsonPropertyName("stars")]
    public required WeatherStar[] Stars { get; set; }

    public class WeatherStar
    {
        [JsonPropertyName("switches")]
        public required string Switches { get; set; }

        /// <summary>
        /// Primary forecast & conditions location
        /// </summary>
        [JsonPropertyName("location")]
        public required string Location { get; set; }

        /// <summary>
        /// Optional: manually specify NWS zones/counties for alerts
        /// </summary>
        [JsonPropertyName("zones")]
        public List<string>? Zones { get; set; }

        [JsonPropertyName("location_name")]
        public required string LocationName { get; set; }
        
        [JsonPropertyName("headend_id")]
        public string? HeadendId { get; set; } = null;
        
        [JsonPropertyName("channel_number")]
        public string? ChannelNumber { get; set; } = null;
        
        [JsonPropertyName("affiliate_name")]
        public string? AffiliateName { get; set; } = null;
        
        [JsonPropertyName("radio_station")]
        public string? RadioStation { get; set; } = null;

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
                Logger.Error("Failed to find time zone: " + ex.Message);
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
    public void Save(string fileName = "config.json")
    {
        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(GetPath(fileName), JsonSerializer.Serialize(this, options));
    }

    public static string GetPath(string fileName = "config.json")
    {
        return Path.Combine(Util.GetExeLocation(), fileName);
    }

    /// <summary>
    /// Load config from config.json
    /// </summary>
    /// <returns></returns>
    public static async Task<Config> LoadConfig(string fileName = "config.json")
    {
        string configPath = GetPath(fileName);
        if (!File.Exists(configPath))
            throw new InvalidOperationException(fileName + " does not exist. Run program with --create-config");

        // Options to allow trailing commas in config.json
        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        string fileContent = await File.ReadAllTextAsync(configPath);
        return JsonSerializer.Deserialize<Config>(fileContent, options) ?? throw new InvalidOperationException("Invalid config.json file");
    }

    public static void CreateConfig(string fileName = "config.json")
    {
        Config newConfig = new Config
        {
            APIKey = "e1f10a1e78da46f5b10a1e78da96f525",
            LoopFlavor = "L",
            ForceClockSet = true,
            Stars =
            [
                new WeatherStar()
                {
                    Location = "40.78,-73.96",
                    LocationName = "Central Park",
                    Switches = "00000000",
                }
            ]
        };
        newConfig.Save(fileName);
        Logger.Info("Created new config file: " + fileName);
    }
}