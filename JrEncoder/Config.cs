﻿using System.Text.Json.Serialization;

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
            return newConfig;
        }

        string fileContent = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<Config>(fileContent) ?? throw new InvalidOperationException("Invalid config.json file");
    }
}