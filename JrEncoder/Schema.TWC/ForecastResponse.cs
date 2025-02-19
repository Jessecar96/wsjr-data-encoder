using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class ForecastResponse
{
    [JsonPropertyName("calendarDayTemperatureMax")]
    public List<int> CalendarDayTemperatureMax { get; set; }

    [JsonPropertyName("calendarDayTemperatureMin")]
    public List<int> CalendarDayTemperatureMin { get; set; }

    [JsonPropertyName("dayOfWeek")]
    public List<string> DayOfWeek { get; set; }

    [JsonPropertyName("expirationTimeUtc")]
    public List<int> ExpirationTimeUtc { get; set; }

    [JsonPropertyName("moonPhase")]
    public List<string> MoonPhase { get; set; }

    [JsonPropertyName("moonPhaseCode")]
    public List<string> MoonPhaseCode { get; set; }

    [JsonPropertyName("moonPhaseDay")]
    public List<int> MoonPhaseDay { get; set; }

    [JsonPropertyName("moonriseTimeLocal")]
    public List<string?> MoonriseTimeLocal { get; set; }

    [JsonPropertyName("moonriseTimeUtc")]
    public List<int?> MoonriseTimeUtc { get; set; }

    [JsonPropertyName("moonsetTimeLocal")]
    public List<string> MoonsetTimeLocal { get; set; }

    [JsonPropertyName("moonsetTimeUtc")]
    public List<int> MoonsetTimeUtc { get; set; }

    [JsonPropertyName("narrative")]
    public List<string> Narrative { get; set; }

    [JsonPropertyName("qpf")]
    public List<double> Qpf { get; set; }

    [JsonPropertyName("qpfSnow")]
    public List<double> QpfSnow { get; set; }

    [JsonPropertyName("sunriseTimeLocal")]
    public List<string> SunriseTimeLocal { get; set; }

    [JsonPropertyName("sunriseTimeUtc")]
    public List<int> SunriseTimeUtc { get; set; }

    [JsonPropertyName("sunsetTimeLocal")]
    public List<string> SunsetTimeLocal { get; set; }

    [JsonPropertyName("sunsetTimeUtc")]
    public List<int> SunsetTimeUtc { get; set; }

    [JsonPropertyName("temperatureMax")]
    public List<int?> TemperatureMax { get; set; }

    [JsonPropertyName("temperatureMin")]
    public List<int> TemperatureMin { get; set; }

    [JsonPropertyName("validTimeLocal")]
    public List<string> ValidTimeLocal { get; set; }

    [JsonPropertyName("validTimeUtc")]
    public List<int> ValidTimeUtc { get; set; }

    [JsonPropertyName("daypart")]
    public List<ForecastDaypart> Daypart { get; set; }
}

public class ForecastDaypart
{
    [JsonPropertyName("cloudCover")]
    public List<int?> CloudCover { get; set; }

    [JsonPropertyName("dayOrNight")]
    public List<string?> DayOrNight { get; set; }

    [JsonPropertyName("daypartName")]
    public List<string?> DaypartName { get; set; }

    [JsonPropertyName("iconCode")]
    public List<int?> IconCode { get; set; }

    [JsonPropertyName("iconCodeExtend")]
    public List<int?> IconCodeExtend { get; set; }

    [JsonPropertyName("narrative")]
    public List<string?> Narrative { get; set; }

    [JsonPropertyName("precipChance")]
    public List<int?> PrecipChance { get; set; }

    [JsonPropertyName("precipType")]
    public List<string?> PrecipType { get; set; }

    [JsonPropertyName("qpf")]
    public List<double?> Qpf { get; set; }

    [JsonPropertyName("qpfSnow")]
    public List<double?> QpfSnow { get; set; }

    [JsonPropertyName("qualifierCode")]
    public List<string?> QualifierCode { get; set; }

    [JsonPropertyName("qualifierPhrase")]
    public List<string?> QualifierPhrase { get; set; }

    [JsonPropertyName("relativeHumidity")]
    public List<int?> RelativeHumidity { get; set; }

    [JsonPropertyName("snowRange")]
    public List<string?> SnowRange { get; set; }

    [JsonPropertyName("temperature")]
    public List<int?> Temperature { get; set; }

    [JsonPropertyName("temperatureHeatIndex")]
    public List<int?> TemperatureHeatIndex { get; set; }

    [JsonPropertyName("temperatureWindChill")]
    public List<int?> TemperatureWindChill { get; set; }

    [JsonPropertyName("thunderCategory")]
    public List<string?> ThunderCategory { get; set; }

    [JsonPropertyName("thunderIndex")]
    public List<int?> ThunderIndex { get; set; }

    [JsonPropertyName("uvDescription")]
    public List<string?> UvDescription { get; set; }

    [JsonPropertyName("uvIndex")]
    public List<int?> UvIndex { get; set; }

    [JsonPropertyName("windDirection")]
    public List<int?> WindDirection { get; set; }

    [JsonPropertyName("windDirectionCardinal")]
    public List<string?> WindDirectionCardinal { get; set; }

    [JsonPropertyName("windPhrase")]
    public List<string?> WindPhrase { get; set; }

    [JsonPropertyName("windSpeed")]
    public List<int?> WindSpeed { get; set; }

    [JsonPropertyName("wxPhraseLong")]
    public List<string?> WxPhraseLong { get; set; }

    [JsonPropertyName("wxPhraseShort")]
    public List<string?> WxPhraseShort { get; set; }
}