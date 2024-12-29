using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class Observations
{
    [JsonPropertyName("cloudCeiling")]
    public string? CloudCeiling { get; set; }

    [JsonPropertyName("cloudCoverPhrase")]
    public string CloudCoverPhrase { get; set; }

    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeek { get; set; }

    [JsonPropertyName("dayOrNight")]
    public string DayOrNight { get; set; }

    [JsonPropertyName("expirationTimeUtc")]
    public int ExpirationTimeUtc { get; set; }

    [JsonPropertyName("iconCode")]
    public int IconCode { get; set; }

    [JsonPropertyName("iconCodeExtend")]
    public int IconCodeExtend { get; set; }

    [JsonPropertyName("obsQualifierCode")]
    public string ObsQualifierCode { get; set; }

    [JsonPropertyName("obsQualifierSeverity")]
    public int ObsQualifierSeverity { get; set; }

    [JsonPropertyName("precip1Hour")]
    public double Precip1Hour { get; set; }

    [JsonPropertyName("precip6Hour")]
    public double Precip6Hour { get; set; }

    [JsonPropertyName("precip24Hour")]
    public double Precip24Hour { get; set; }

    [JsonPropertyName("pressureAltimeter")]
    public double PressureAltimeter { get; set; }

    [JsonPropertyName("pressureChange")]
    public double PressureChange { get; set; }

    [JsonPropertyName("pressureMeanSeaLevel")]
    public double PressureMeanSeaLevel { get; set; }

    [JsonPropertyName("pressureTendencyCode")]
    public int PressureTendencyCode { get; set; }

    [JsonPropertyName("pressureTendencyTrend")]
    public string PressureTendencyTrend { get; set; }

    [JsonPropertyName("relativeHumidity")]
    public int RelativeHumidity { get; set; }

    [JsonPropertyName("snow1Hour")]
    public double Snow1Hour { get; set; }

    [JsonPropertyName("snow6Hour")]
    public double Snow6Hour { get; set; }

    [JsonPropertyName("snow24Hour")]
    public double Snow24Hour { get; set; }

    [JsonPropertyName("sunriseTimeLocal")]
    public DateTime SunriseTimeLocal { get; set; }

    [JsonPropertyName("sunriseTimeUtc")]
    public int SunriseTimeUtc { get; set; }

    [JsonPropertyName("sunsetTimeLocal")]
    public DateTime SunsetTimeLocal { get; set; }

    [JsonPropertyName("sunsetTimeUtc")]
    public int SunsetTimeUtc { get; set; }

    [JsonPropertyName("temperature")]
    public int Temperature { get; set; }

    [JsonPropertyName("temperatureChange24Hour")]
    public int TemperatureChange24Hour { get; set; }

    [JsonPropertyName("temperatureDewPoint")]
    public int TemperatureDewPoint { get; set; }

    [JsonPropertyName("temperatureFeelsLike")]
    public int TemperatureFeelsLike { get; set; }

    [JsonPropertyName("temperatureHeatIndex")]
    public int TemperatureHeatIndex { get; set; }

    [JsonPropertyName("temperatureMax24Hour")]
    public int TemperatureMax24Hour { get; set; }

    [JsonPropertyName("temperatureMaxSince7Am")]
    public int TemperatureMaxSince7Am { get; set; }

    [JsonPropertyName("temperatureMin24Hour")]
    public int TemperatureMin24Hour { get; set; }

    [JsonPropertyName("temperatureWindChill")]
    public int TemperatureWindChill { get; set; }

    [JsonPropertyName("uvDescription")]
    public string UvDescription { get; set; }

    [JsonPropertyName("uvIndex")]
    public int UvIndex { get; set; }

    [JsonPropertyName("validTimeLocal")]
    public DateTime ValidTimeLocal { get; set; }

    [JsonPropertyName("validTimeUtc")]
    public int ValidTimeUtc { get; set; }

    [JsonPropertyName("visibility")]
    public double Visibility { get; set; }

    [JsonPropertyName("windDirection")]
    public int WindDirection { get; set; }

    [JsonPropertyName("windDirectionCardinal")]
    public string WindDirectionCardinal { get; set; }

    [JsonPropertyName("windGust")]
    public int WindGust { get; set; }

    [JsonPropertyName("windSpeed")]
    public int WindSpeed { get; set; }

    [JsonPropertyName("wxPhraseLong")]
    public string WxPhraseLong { get; set; }

    [JsonPropertyName("wxPhraseMedium")]
    public string WxPhraseMedium { get; set; }

    [JsonPropertyName("wxPhraseShort")]
    public string WxPhraseShort { get; set; }
}