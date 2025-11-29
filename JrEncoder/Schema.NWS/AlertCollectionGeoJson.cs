using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JrEncoder.Schema.NWS;

public class GeoJsonFeature
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("geometry")]
    public object? Geometry { get; init; }

    [JsonPropertyName("properties")]
    public required Alert Alert { get; init; }
}

public class Geocode
{
    [JsonPropertyName("SAME")]
    public required List<string> SAME { get; init; }

    [JsonPropertyName("UGC")]
    public required List<string> UGC { get; init; }
}

public class Parameters
{
    [JsonPropertyName("AWIPSidentifier")]
    public required List<string> AWIPSidentifier { get; init; }

    [JsonPropertyName("WMOidentifier")]
    public required List<string> WMOidentifier { get; init; }

    [JsonPropertyName("NWSheadline")]
    public List<string>? NWSheadline { get; init; }

    [JsonPropertyName("BLOCKCHANNEL")]
    public required List<string> BLOCKCHANNEL { get; init; }

    [JsonPropertyName("EAS-ORG")]
    public required List<string> EASORG { get; init; }

    [JsonPropertyName("VTEC")]
    public required List<string> VTEC { get; init; }

    [JsonPropertyName("eventEndingTime")]
    public required List<DateTime> EventEndingTime { get; init; }
}

public class Alert
{
    /// <summary>
    /// The identifier of the alert message.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// A textual description of the area affected by the alert.
    /// </summary>
    [JsonPropertyName("areaDesc")]
    public required string AreaDesc { get; init; }

    /// <summary>
    /// Lists of codes for NWS public zones and counties affected by the alert.
    /// </summary>
    [JsonPropertyName("geocode")]
    public required Geocode Geocode { get; init; }

    /// <summary>
    /// An array of API links for zones affected by the alert. This is an API-specific extension field and is not part of the CAP specification.
    /// </summary>
    [JsonPropertyName("affectedZones")]
    public required List<string> AffectedZones { get; init; }

    /// <summary>
    /// A list of prior alerts that this alert updates or replaces.
    /// </summary>
    [JsonPropertyName("references")]
    public required List<Reference> References { get; init; }

    /// <summary>
    /// The time of the origination of the alert message.
    /// </summary>
    [JsonPropertyName("sent")]
    public required DateTimeOffset Sent { get; init; }

    /// <summary>
    /// The effective time of the information of the alert message.
    /// </summary>
    [JsonPropertyName("effective")]
    public required DateTimeOffset Effective { get; init; }

    /// <summary>
    /// The expected time of the beginning of the subject event of the alert message.
    /// </summary>
    [JsonPropertyName("onset")]
    public required DateTimeOffset? Onset { get; init; }

    /// <summary>
    /// The expiry time of the information of the alert message.
    /// </summary>
    [JsonPropertyName("expires")]
    public required DateTimeOffset Expires { get; init; }

    /// <summary>
    /// The expected end time of the subject event of the alert message.
    /// </summary>
    [JsonPropertyName("ends")]
    public required DateTimeOffset? Ends { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("messageType")]
    public required string MessageType { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    [JsonPropertyName("certainty")]
    public required string Certainty { get; init; }

    [JsonPropertyName("urgency")]
    public required string Urgency { get; init; }

    /// <summary>
    /// The text denoting the type of the subject event of the alert message.
    /// </summary>
    [JsonPropertyName("event")]
    public required string Event { get; init; }

    /// <summary>
    /// Email address of the NWS webmaster.
    /// </summary>
    [JsonPropertyName("sender")]
    public required string Sender { get; init; }

    /// <summary>
    /// The text naming the originator of the alert message.
    /// </summary>
    [JsonPropertyName("senderName")]
    public required string SenderName { get; init; }

    /// <summary>
    /// The text headline of the alert message.
    /// </summary>
    [JsonPropertyName("headline")]
    public required string? Headline { get; init; }

    /// <summary>
    /// The text describing the subject event of the alert message.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// The text describing the recommended action to be taken by recipients of the alert message.
    /// </summary>
    [JsonPropertyName("instruction")]
    public required string? Instruction { get; init; }

    /// <summary>
    /// The code denoting the type of action recommended for the target audience. This corresponds to responseType in the CAP specification.
    /// </summary>
    [JsonPropertyName("response")]
    public required string Response { get; init; }

    /// <summary>
    /// System-specific additional parameters associated with the alert message. The keys in this object correspond to parameter definitions in the NWS CAP specification.
    /// </summary>
    [JsonPropertyName("parameters")]
    public required Parameters Parameters { get; init; }

    /// <summary>
    /// Get weather star formatted headline
    /// </summary>
    /// <returns></returns>
    public string GetHeadline(TimeZoneInfo zoneInfo)
    {
        // Custom handling for Tornado Watch
        if (Event == "Tornado Watch")
        {
            // Extract watch number from description text
            string watchNumber = "";
            MatchCollection m = Regex.Matches(Description.Replace("\n", " "), @"TORNADO WATCH (\d+)");
            if (m.Count != 0 && m[0].Groups.Count != 0)
            {
                // If that regex matches use group 1 to extract the clean version
                watchNumber = m[0].Groups[1].Value;
            }

            // Add space before if we were able to find a watch number
            if (!string.IsNullOrEmpty(watchNumber))
                watchNumber = " " + watchNumber;
            
            // No space before watchNumber just in case we don't find it
            return ($"TORNADO WATCH{watchNumber} IN EFFECT UNTIL " + GetEndTimeString(zoneInfo)).ToUpper();
        }

        // Everything else
        return ($"{Event} IN EFFECT UNTIL " + GetEndTimeString(zoneInfo)).ToUpper();
    }

    /// <summary>
    /// Get ending time string for headline string
    /// </summary>
    /// <returns></returns>
    private string GetEndTimeString(TimeZoneInfo zoneInfo)
    {
        // Use "Ends" if we have it, or else use "Expires"
        DateTimeOffset end = Ends ?? Expires;

        if (end.Date == DateTime.Today)
        {
            // Expires today, only give ending time
            string tzAbrev = Util.GetTimeZoneAbbreviation(zoneInfo);
            return end.ToString("h tt", CultureInfo.InvariantCulture) + " " + tzAbrev;
        }
        else
        {
            // Expires after today, include day in the text
            string tzAbrev = Util.GetTimeZoneAbbreviation(zoneInfo);
            return end.ToString("dddd h tt", CultureInfo.InvariantCulture) + " " + tzAbrev;
        }
    }
}

public class Reference
{
    [JsonPropertyName("@id")]
    public required string Id { get; init; }

    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }

    [JsonPropertyName("sender")]
    public required string Sender { get; init; }

    [JsonPropertyName("sent")]
    public required DateTime Sent { get; init; }
}

/// <summary>
/// https://rest.wiki/?https://api.weather.gov/openapi.json
/// </summary>
public class AlertCollectionGeoJson
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// A GeoJSON feature. Please refer to IETF RFC 7946 for information on the GeoJSON format.
    /// </summary>
    [JsonPropertyName("features")]
    public required List<GeoJsonFeature> Features { get; init; }

    /// <summary>
    /// A title describing the alert collection
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// The last time a change occurred to this collection
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime? Updated { get; init; }
}