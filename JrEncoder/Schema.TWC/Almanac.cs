using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class Almanac
{
    [JsonPropertyName("almanacInterval")]
    public List<string> AlmanacInterval { get; set; }

    [JsonPropertyName("almanacRecordDate")]
    public List<string> AlmanacRecordDate { get; set; }

    [JsonPropertyName("almanacRecordPeriod")]
    public List<int> AlmanacRecordPeriod { get; set; }

    [JsonPropertyName("almanacRecordYearMax")]
    public List<int> AlmanacRecordYearMax { get; set; }

    [JsonPropertyName("almanacRecordYearMin")]
    public List<int> AlmanacRecordYearMin { get; set; }

    [JsonPropertyName("precipitationAverage")]
    public List<double?> PrecipitationAverage { get; set; }

    [JsonPropertyName("snowAccumulationAverage")]
    public List<double?> SnowAccumulationAverage { get; set; }

    [JsonPropertyName("stationId")]
    public List<string> StationId { get; set; }

    [JsonPropertyName("stationName")]
    public List<string> StationName { get; set; }

    [JsonPropertyName("temperatureAverageMax")]
    public List<int> TemperatureAverageMax { get; set; }

    [JsonPropertyName("temperatureAverageMin")]
    public List<int> TemperatureAverageMin { get; set; }

    [JsonPropertyName("temperatureMean")]
    public List<int> TemperatureMean { get; set; }

    [JsonPropertyName("temperatureRecordMax")]
    public List<int> TemperatureRecordMax { get; set; }

    [JsonPropertyName("temperatureRecordMin")]
    public List<int> TemperatureRecordMin { get; set; }
}