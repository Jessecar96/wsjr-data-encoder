using System.Text.Json.Serialization;

namespace JrEncoder.Schema.NWS;

public class PointGeoJson
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("properties")]
    public required Point Properties { get; init; }
}

public class Point
{
    /// <summary>
    /// Three-letter identifier for a NWS office.
    /// </summary>
    [JsonPropertyName("cwa")]
    public required string Cwa { get; init; }

    [JsonPropertyName("forecastOffice")]
    public required string ForecastOffice { get; init; }

    [JsonPropertyName("gridId")]
    public required string GridId { get; init; }

    [JsonPropertyName("gridX")]
    public required int GridX { get; init; }

    [JsonPropertyName("gridY")]
    public required int GridY { get; init; }

    [JsonPropertyName("forecast")]
    public required string Forecast { get; init; }

    [JsonPropertyName("forecastHourly")]
    public required string ForecastHourly { get; init; }

    [JsonPropertyName("forecastGridData")]
    public required string ForecastGridData { get; init; }

    [JsonPropertyName("observationStations")]
    public required string ObservationStations { get; init; }

    [JsonPropertyName("forecastZone")]
    public required string ForecastZone { get; init; }

    [JsonPropertyName("county")]
    public required string County { get; init; }

    [JsonPropertyName("fireWeatherZone")]
    public required string FireWeatherZone { get; init; }

    [JsonPropertyName("timeZone")]
    public required string TimeZone { get; init; }

    [JsonPropertyName("radarStation")]
    public required string RadarStation { get; init; }
}