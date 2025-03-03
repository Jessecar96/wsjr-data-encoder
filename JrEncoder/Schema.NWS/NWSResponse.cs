using System.Text.Json.Serialization;

namespace JrEncoder.Schema.NWS;

public class NWSFeature
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("geometry")]
    public object Geometry { get; set; }

    [JsonPropertyName("properties")]
    public Properties Properties { get; set; }
}

public class Geocode
{
    [JsonPropertyName("SAME")]
    public List<string> SAME { get; set; }

    [JsonPropertyName("UGC")]
    public List<string> UGC { get; set; }
}

public class Parameters
{
    [JsonPropertyName("AWIPSidentifier")]
    public List<string> AWIPSidentifier { get; set; }

    [JsonPropertyName("WMOidentifier")]
    public List<string> WMOidentifier { get; set; }

    [JsonPropertyName("NWSheadline")]
    public List<string> NWSheadline { get; set; }

    [JsonPropertyName("BLOCKCHANNEL")]
    public List<string> BLOCKCHANNEL { get; set; }

    [JsonPropertyName("EAS-ORG")]
    public List<string> EASORG { get; set; }

    [JsonPropertyName("VTEC")]
    public List<string> VTEC { get; set; }

    [JsonPropertyName("eventEndingTime")]
    public List<DateTime> EventEndingTime { get; set; }
}

public class Properties
{
    [JsonPropertyName("@id")]
    public string AtId { get; set; }

    [JsonPropertyName("@type")]
    public string AtType { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("areaDesc")]
    public string AreaDesc { get; set; }

    [JsonPropertyName("geocode")]
    public Geocode Geocode { get; set; }

    [JsonPropertyName("affectedZones")]
    public List<string> AffectedZones { get; set; }

    [JsonPropertyName("references")]
    public List<Reference> References { get; set; }

    [JsonPropertyName("sent")]
    public DateTime Sent { get; set; }

    [JsonPropertyName("effective")]
    public DateTime Effective { get; set; }

    [JsonPropertyName("onset")]
    public DateTime Onset { get; set; }

    [JsonPropertyName("expires")]
    public DateTime Expires { get; set; }

    [JsonPropertyName("ends")]
    public DateTime Ends { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("messageType")]
    public string MessageType { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; }

    [JsonPropertyName("certainty")]
    public string Certainty { get; set; }

    [JsonPropertyName("urgency")]
    public string Urgency { get; set; }

    [JsonPropertyName("event")]
    public string Event { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("senderName")]
    public string SenderName { get; set; }

    [JsonPropertyName("headline")]
    public string Headline { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("instruction")]
    public string Instruction { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; }
}

public class Reference
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("sent")]
    public DateTime Sent { get; set; }
}

public class NWSResponse
{
    //[JsonPropertyName("@context")]
    //public List<object> Context { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("features")]
    public List<NWSFeature> Features { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }
}