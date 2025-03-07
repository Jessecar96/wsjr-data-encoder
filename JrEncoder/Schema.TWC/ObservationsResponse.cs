using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class ObservationsResponse
{
    [JsonPropertyName("cloudCeiling")]
    public int? CloudCeiling { get; init; }

    [JsonPropertyName("cloudCoverPhrase")]
    public string? CloudCoverPhrase { get; init; }

    [JsonPropertyName("dayOfWeek")]
    public string? DayOfWeek { get; init; }

    [JsonPropertyName("dayOrNight")]
    public string? DayOrNight { get; init; }

    [JsonPropertyName("expirationTimeUtc")]
    public int? ExpirationTimeUtc { get; init; }

    [JsonPropertyName("iconCode")]
    public int? IconCode { get; init; }

    [JsonPropertyName("iconCodeExtend")]
    public int? IconCodeExtend { get; init; }

    [JsonPropertyName("obsQualifierCode")]
    public string? ObsQualifierCode { get; init; }

    [JsonPropertyName("obsQualifierSeverity")]
    public int? ObsQualifierSeverity { get; init; }

    [JsonPropertyName("precip1Hour")]
    public double? Precip1Hour { get; init; }

    [JsonPropertyName("precip6Hour")]
    public double? Precip6Hour { get; init; }

    [JsonPropertyName("precip24Hour")]
    public double? Precip24Hour { get; init; }

    [JsonPropertyName("pressureAltimeter")]
    public double? PressureAltimeter { get; init; }

    [JsonPropertyName("pressureChange")]
    public double? PressureChange { get; init; }

    [JsonPropertyName("pressureMeanSeaLevel")]
    public double? PressureMeanSeaLevel { get; init; }

    [JsonPropertyName("pressureTendencyCode")]
    public int? PressureTendencyCode { get; init; }

    [JsonPropertyName("pressureTendencyTrend")]
    public string? PressureTendencyTrend { get; init; }

    [JsonPropertyName("relativeHumidity")]
    public int? RelativeHumidity { get; init; }

    [JsonPropertyName("snow1Hour")]
    public double? Snow1Hour { get; init; }

    [JsonPropertyName("snow6Hour")]
    public double? Snow6Hour { get; init; }

    [JsonPropertyName("snow24Hour")]
    public double? Snow24Hour { get; init; }

    [JsonPropertyName("sunriseTimeLocal")]
    public string? SunriseTimeLocal { get; init; }

    [JsonPropertyName("sunriseTimeUtc")]
    public int? SunriseTimeUtc { get; init; }

    [JsonPropertyName("sunsetTimeLocal")]
    public string? SunsetTimeLocal { get; init; }

    [JsonPropertyName("sunsetTimeUtc")]
    public int? SunsetTimeUtc { get; init; }

    [JsonPropertyName("temperature")]
    public int? Temperature { get; init; }

    [JsonPropertyName("temperatureChange24Hour")]
    public int? TemperatureChange24Hour { get; init; }

    [JsonPropertyName("temperatureDewPoint")]
    public int? TemperatureDewPoint { get; init; }

    [JsonPropertyName("temperatureFeelsLike")]
    public int? TemperatureFeelsLike { get; init; }

    [JsonPropertyName("temperatureHeatIndex")]
    public int? TemperatureHeatIndex { get; init; }

    [JsonPropertyName("temperatureMax24Hour")]
    public int? TemperatureMax24Hour { get; init; }

    [JsonPropertyName("temperatureMaxSince7Am")]
    public int? TemperatureMaxSince7Am { get; init; }

    [JsonPropertyName("temperatureMin24Hour")]
    public int? TemperatureMin24Hour { get; init; }

    [JsonPropertyName("temperatureWindChill")]
    public int? TemperatureWindChill { get; init; }

    [JsonPropertyName("uvDescription")]
    public string? UvDescription { get; init; }

    [JsonPropertyName("uvIndex")]
    public int? UvIndex { get; init; }

    [JsonPropertyName("validTimeLocal")]
    public string? ValidTimeLocal { get; init; }

    [JsonPropertyName("validTimeUtc")]
    public int? ValidTimeUtc { get; init; }

    [JsonPropertyName("visibility")]
    public double? Visibility { get; init; }

    [JsonPropertyName("windDirection")]
    public int? WindDirection { get; init; }

    [JsonPropertyName("windDirectionCardinal")]
    public string? WindDirectionCardinal { get; init; }

    [JsonPropertyName("windGust")]
    public int? WindGust { get; init; }

    [JsonPropertyName("windSpeed")]
    public int? WindSpeed { get; init; }

    [JsonPropertyName("wxPhraseLong")]
    public string? WxPhraseLong { get; init; }

    [JsonPropertyName("wxPhraseMedium")]
    public string? WxPhraseMedium { get; init; }

    [JsonPropertyName("wxPhraseShort")]
    public string? WxPhraseShort { get; init; }
    
    public string GetFormattedWxPhraseShort()
    {
        if (WxPhraseShort == null) return "";

        string output = WxPhraseShort ?? "";

        // Remove anything after the / which is usually a second condition
        if (output.Contains('/'))
            output = output.Substring(0, output.IndexOf("/"));

        return output;
    }
}