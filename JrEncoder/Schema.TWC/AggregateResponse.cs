using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

/// <summary>
/// Schema for TWC's aggregate API
/// https://api.weather.com/v3/aggcommon/[semicolon-separated product list]?geocodes=[geocode];[geocode]&language=en-US&format=json&units=e&apiKey=[yourApiKey]
/// </summary>
public class AggregateResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("v3-wx-observations-current")]
    public ObservationsResponse? ObservationsResponse { get; init; }
}