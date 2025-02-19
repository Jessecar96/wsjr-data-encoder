using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class HeadlinesResponse
{
    [JsonPropertyName("metadata")]
    public required MetadataClass Metadata { get; init; }

    [JsonPropertyName("alerts")]
    public required List<Alert> Alerts { get; init; }

    public class CategoryClass
    {
        [JsonPropertyName("category")]
        public string? Category { get; init; }

        [JsonPropertyName("categoryCode")]
        public int? CategoryCode { get; init; }
    }

    public class MetadataClass
    {
        [JsonPropertyName("next")]
        public string? Next { get; init; }
    }

    public class ResponseTypeClass
    {
        [JsonPropertyName("responseType")]
        public string? ResponseType { get; init; }

        [JsonPropertyName("responseTypeCode")]
        public int? ResponseTypeCode { get; init; }
    }

    public class Alert
    {
        [JsonPropertyName("detailKey")]
        public required string DetailKey { get; init; }

        [JsonPropertyName("messageTypeCode")]
        public int? MessageTypeCode { get; init; }

        [JsonPropertyName("messageType")]
        public string? MessageType { get; init; }

        [JsonPropertyName("productIdentifier")]
        public string? ProductIdentifier { get; init; }

        [JsonPropertyName("phenomena")]
        public string? Phenomena { get; init; }

        [JsonPropertyName("significance")]
        public string? Significance { get; init; }

        [JsonPropertyName("eventTrackingNumber")]
        public string? EventTrackingNumber { get; init; }

        [JsonPropertyName("officeCode")]
        public string? OfficeCode { get; init; }

        [JsonPropertyName("officeName")]
        public string? OfficeName { get; init; }

        [JsonPropertyName("officeAdminDistrict")]
        public string? OfficeAdminDistrict { get; init; }

        [JsonPropertyName("officeAdminDistrictCode")]
        public string? OfficeAdminDistrictCode { get; init; }

        [JsonPropertyName("officeCountryCode")]
        public string? OfficeCountryCode { get; init; }

        [JsonPropertyName("eventDescription")]
        public string? EventDescription { get; init; }

        [JsonPropertyName("severityCode")]
        public int? SeverityCode { get; init; }

        [JsonPropertyName("severity")]
        public string? Severity { get; init; }

        [JsonPropertyName("categories")]
        public required List<CategoryClass> Categories { get; init; }

        [JsonPropertyName("responseTypes")]
        public required List<ResponseTypeClass> ResponseTypes { get; init; }

        [JsonPropertyName("urgency")]
        public string? Urgency { get; init; }

        [JsonPropertyName("urgencyCode")]
        public int? UrgencyCode { get; init; }

        [JsonPropertyName("certainty")]
        public string? Certainty { get; init; }

        [JsonPropertyName("certaintyCode")]
        public int? CertaintyCode { get; init; }

        [JsonPropertyName("effectiveTimeLocal")]
        public string? EffectiveTimeLocal { get; init; }

        [JsonPropertyName("effectiveTimeLocalTimeZone")]
        public string? EffectiveTimeLocalTimeZone { get; init; }

        [JsonPropertyName("expireTimeLocal")]
        public string? ExpireTimeLocal { get; init; }

        [JsonPropertyName("expireTimeLocalTimeZone")]
        public string? ExpireTimeLocalTimeZone { get; init; }

        [JsonPropertyName("expireTimeUTC")]
        public int? ExpireTimeUTC { get; init; }

        [JsonPropertyName("onsetTimeLocal")]
        public string? OnsetTimeLocal { get; init; }

        [JsonPropertyName("onsetTimeLocalTimeZone")]
        public string? OnsetTimeLocalTimeZone { get; init; }

        [JsonPropertyName("flood")]
        public string? Flood { get; init; }

        [JsonPropertyName("areaTypeCode")]
        public string? AreaTypeCode { get; init; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; init; }

        [JsonPropertyName("areaId")]
        public string? AreaId { get; init; }

        [JsonPropertyName("areaName")]
        public string? AreaName { get; init; }

        [JsonPropertyName("ianaTimeZone")]
        public string? IanaTimeZone { get; init; }

        [JsonPropertyName("adminDistrictCode")]
        public string? AdminDistrictCode { get; init; }

        [JsonPropertyName("adminDistrict")]
        public string? AdminDistrict { get; init; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; init; }

        [JsonPropertyName("countryName")]
        public string? CountryName { get; init; }

        [JsonPropertyName("headlineText")]
        public string? HeadlineText { get; init; }

        [JsonPropertyName("source")]
        public string? Source { get; init; }

        [JsonPropertyName("disclaimer")]
        public object? Disclaimer { get; init; }

        [JsonPropertyName("issueTimeLocal")]
        public string? IssueTimeLocal { get; init; }

        [JsonPropertyName("issueTimeLocalTimeZone")]
        public string? IssueTimeLocalTimeZone { get; init; }

        [JsonPropertyName("identifier")]
        public string? Identifier { get; init; }

        [JsonPropertyName("processTimeUTC")]
        public int? ProcessTimeUTC { get; init; }

        [JsonPropertyName("endTimeLocal")]
        public string? EndTimeLocal { get; init; }

        [JsonPropertyName("endTimeLocalTimeZone")]
        public string? EndTimeLocalTimeZone { get; init; }

        [JsonPropertyName("endTimeUTC")]
        public int? EndTimeUTC { get; init; }

        [JsonPropertyName("displayRank")]
        public int? DisplayRank { get; init; }

        /// <summary>
        /// If this alert should show as a headline
        /// </summary>
        /// <returns></returns>
        public bool ShowAsHeadline()
        {
            string[] doNotShow = ["MWW"];
            return !doNotShow.Contains(this.ProductIdentifier);
        }
    }
}