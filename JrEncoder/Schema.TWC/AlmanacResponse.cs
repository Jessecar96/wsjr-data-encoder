using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class AlmanacResponse
{
    [JsonPropertyName("almanacInterval")]
    public required List<string?> AlmanacInterval { get; init; }

    [JsonPropertyName("almanacRecordDate")]
    public required List<string?> AlmanacRecordDate { get; init; }

    [JsonPropertyName("almanacRecordPeriod")]
    public required List<int?> AlmanacRecordPeriod { get; init; }

    [JsonPropertyName("almanacRecordYearMax")]
    public required List<int?> AlmanacRecordYearMax { get; init; }

    [JsonPropertyName("almanacRecordYearMin")]
    public required List<int?> AlmanacRecordYearMin { get; init; }

    [JsonPropertyName("precipitationAverage")]
    public required List<double?> PrecipitationAverage { get; init; }

    [JsonPropertyName("snowAccumulationAverage")]
    public required List<double?> SnowAccumulationAverage { get; init; }

    [JsonPropertyName("stationId")]
    public required List<string?> StationId { get; init; }

    [JsonPropertyName("stationName")]
    public required List<string?> StationName { get; init; }

    [JsonPropertyName("temperatureAverageMax")]
    public required List<int?> TemperatureAverageMax { get; init; }

    [JsonPropertyName("temperatureAverageMin")]
    public required List<int?> TemperatureAverageMin { get; init; }

    [JsonPropertyName("temperatureMean")]
    public required List<int?> TemperatureMean { get; init; }

    [JsonPropertyName("temperatureRecordMax")]
    public required List<int?> TemperatureRecordMax { get; init; }

    [JsonPropertyName("temperatureRecordMin")]
    public required List<int?> TemperatureRecordMin { get; init; }
}