using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class ForecastResponse
{
    [JsonPropertyName("calendarDayTemperatureMax")]
    public required List<int?> CalendarDayTemperatureMax { get; init; }

    [JsonPropertyName("calendarDayTemperatureMin")]
    public required List<int?> CalendarDayTemperatureMin { get; init; }

    [JsonPropertyName("dayOfWeek")]
    public required List<string> DayOfWeek { get; init; }

    [JsonPropertyName("expirationTimeUtc")]
    public required List<int> ExpirationTimeUtc { get; init; }

    [JsonPropertyName("moonPhase")]
    public required List<string?> MoonPhase { get; init; }

    [JsonPropertyName("moonPhaseCode")]
    public required List<string?> MoonPhaseCode { get; init; }

    [JsonPropertyName("moonPhaseDay")]
    public required List<int?> MoonPhaseDay { get; init; }

    [JsonPropertyName("moonriseTimeLocal")]
    public required List<string?> MoonriseTimeLocal { get; init; }

    [JsonPropertyName("moonriseTimeUtc")]
    public required List<int?> MoonriseTimeUtc { get; init; }

    [JsonPropertyName("moonsetTimeLocal")]
    public required List<string?> MoonsetTimeLocal { get; init; }

    [JsonPropertyName("moonsetTimeUtc")]
    public required List<int?> MoonsetTimeUtc { get; init; }

    [JsonPropertyName("narrative")]
    public required List<string?> Narrative { get; init; }

    [JsonPropertyName("qpf")]
    public required List<double?> Qpf { get; init; }

    [JsonPropertyName("qpfSnow")]
    public required List<double?> QpfSnow { get; init; }

    [JsonPropertyName("sunriseTimeLocal")]
    public required List<string?> SunriseTimeLocal { get; init; }

    [JsonPropertyName("sunriseTimeUtc")]
    public required List<int?> SunriseTimeUtc { get; init; }

    [JsonPropertyName("sunsetTimeLocal")]
    public required List<string?> SunsetTimeLocal { get; init; }

    [JsonPropertyName("sunsetTimeUtc")]
    public required List<int?> SunsetTimeUtc { get; init; }

    [JsonPropertyName("temperatureMax")]
    public required List<int?> TemperatureMax { get; init; }

    [JsonPropertyName("temperatureMin")]
    public required List<int?> TemperatureMin { get; init; }

    [JsonPropertyName("validTimeLocal")]
    public required List<string?> ValidTimeLocal { get; init; }

    [JsonPropertyName("validTimeUtc")]
    public required List<int?> ValidTimeUtc { get; init; }

    [JsonPropertyName("daypart")]
    public required List<ForecastDaypart> Daypart { get; init; }
}

public class ForecastDaypart
{
    [JsonPropertyName("cloudCover")]
    public required List<int?> CloudCover { get; init; }

    [JsonPropertyName("dayOrNight")]
    public required List<string?> DayOrNight { get; init; }

    [JsonPropertyName("daypartName")]
    public required List<string?> DaypartName { get; init; }

    [JsonPropertyName("iconCode")]
    public required List<int?> IconCode { get; init; }

    [JsonPropertyName("iconCodeExtend")]
    public required List<int?> IconCodeExtend { get; init; }

    [JsonPropertyName("narrative")]
    public required List<string?> Narrative { get; init; }

    [JsonPropertyName("precipChance")]
    public required List<int?> PrecipChance { get; init; }

    [JsonPropertyName("precipType")]
    public required List<string?> PrecipType { get; init; }

    [JsonPropertyName("qpf")]
    public required List<double?> Qpf { get; init; }

    [JsonPropertyName("qpfSnow")]
    public required List<double?> QpfSnow { get; init; }

    [JsonPropertyName("qualifierCode")]
    public required List<string?> QualifierCode { get; init; }

    [JsonPropertyName("qualifierPhrase")]
    public required List<string?> QualifierPhrase { get; init; }

    [JsonPropertyName("relativeHumidity")]
    public required List<int?> RelativeHumidity { get; init; }

    [JsonPropertyName("snowRange")]
    public required List<string?> SnowRange { get; init; }

    [JsonPropertyName("temperature")]
    public required List<int?> Temperature { get; init; }

    [JsonPropertyName("temperatureHeatIndex")]
    public required List<int?> TemperatureHeatIndex { get; init; }

    [JsonPropertyName("temperatureWindChill")]
    public required List<int?> TemperatureWindChill { get; init; }

    [JsonPropertyName("thunderCategory")]
    public required List<string?> ThunderCategory { get; init; }

    [JsonPropertyName("thunderIndex")]
    public required List<int?> ThunderIndex { get; init; }

    [JsonPropertyName("uvDescription")]
    public required List<string?> UvDescription { get; init; }

    [JsonPropertyName("uvIndex")]
    public required List<int?> UvIndex { get; init; }

    [JsonPropertyName("windDirection")]
    public required List<int?> WindDirection { get; init; }

    [JsonPropertyName("windDirectionCardinal")]
    public required List<string?> WindDirectionCardinal { get; init; }

    [JsonPropertyName("windPhrase")]
    public required List<string?> WindPhrase { get; init; }

    [JsonPropertyName("windSpeed")]
    public required List<int?> WindSpeed { get; init; }

    [JsonPropertyName("wxPhraseLong")]
    public required List<string?> WxPhraseLong { get; init; }

    [JsonPropertyName("wxPhraseShort")]
    public required List<string?> WxPhraseShort { get; init; }

    public string GetFormattedWxPhrase(int index)
    {
        if (WxPhraseLong[index] == null) return "";
        
        string output = WxPhraseLong[index] ?? "";
        
        // Remove anything after the / which is usually a second condition
        if (output.Contains('/'))
            output = output.Substring(0, output.IndexOf("/"));
        
        return output;
    }
}