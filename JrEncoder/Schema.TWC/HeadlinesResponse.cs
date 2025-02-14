using System.Text.Json.Serialization;

namespace JrEncoder.Schema.TWC;

public class HeadlinesResponse
{
    [JsonPropertyName("metadata")]
    public MetadataClass Metadata { get; set; }

    [JsonPropertyName("alerts")]
    public List<Alert> Alerts { get; set; }

    public class CategoryClass
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("categoryCode")]
        public int CategoryCode { get; set; }
    }

    public class MetadataClass
    {
        [JsonPropertyName("next")]
        public object Next { get; set; }
    }

    public class ResponseTypeClass
    {
        [JsonPropertyName("responseType")]
        public string ResponseType { get; set; }

        [JsonPropertyName("responseTypeCode")]
        public int ResponseTypeCode { get; set; }
    }

    public class Alert
    {
        [JsonPropertyName("detailKey")]
        public string DetailKey { get; set; }

        [JsonPropertyName("messageTypeCode")]
        public int MessageTypeCode { get; set; }

        [JsonPropertyName("messageType")]
        public string MessageType { get; set; }

        [JsonPropertyName("productIdentifier")]
        public string ProductIdentifier { get; set; }

        [JsonPropertyName("phenomena")]
        public string Phenomena { get; set; }

        [JsonPropertyName("significance")]
        public string Significance { get; set; }

        [JsonPropertyName("eventTrackingNumber")]
        public string EventTrackingNumber { get; set; }

        [JsonPropertyName("officeCode")]
        public string OfficeCode { get; set; }

        [JsonPropertyName("officeName")]
        public string OfficeName { get; set; }

        [JsonPropertyName("officeAdminDistrict")]
        public string OfficeAdminDistrict { get; set; }

        [JsonPropertyName("officeAdminDistrictCode")]
        public string OfficeAdminDistrictCode { get; set; }

        [JsonPropertyName("officeCountryCode")]
        public string OfficeCountryCode { get; set; }

        [JsonPropertyName("eventDescription")]
        public string EventDescription { get; set; }

        [JsonPropertyName("severityCode")]
        public int SeverityCode { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("categories")]
        public List<CategoryClass> Categories { get; set; }

        [JsonPropertyName("responseTypes")]
        public List<ResponseTypeClass> ResponseTypes { get; set; }

        [JsonPropertyName("urgency")]
        public string Urgency { get; set; }

        [JsonPropertyName("urgencyCode")]
        public int UrgencyCode { get; set; }

        [JsonPropertyName("certainty")]
        public string Certainty { get; set; }

        [JsonPropertyName("certaintyCode")]
        public int CertaintyCode { get; set; }

        [JsonPropertyName("effectiveTimeLocal")]
        public string? EffectiveTimeLocal { get; set; }

        [JsonPropertyName("effectiveTimeLocalTimeZone")]
        public string? EffectiveTimeLocalTimeZone { get; set; }

        [JsonPropertyName("expireTimeLocal")]
        public string? ExpireTimeLocal { get; set; }

        [JsonPropertyName("expireTimeLocalTimeZone")]
        public string? ExpireTimeLocalTimeZone { get; set; }

        [JsonPropertyName("expireTimeUTC")]
        public int ExpireTimeUTC { get; set; }

        [JsonPropertyName("onsetTimeLocal")]
        public string? OnsetTimeLocal { get; set; }

        [JsonPropertyName("onsetTimeLocalTimeZone")]
        public string? OnsetTimeLocalTimeZone { get; set; }

        [JsonPropertyName("flood")]
        public string? Flood { get; set; }

        [JsonPropertyName("areaTypeCode")]
        public string AreaTypeCode { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("areaId")]
        public string AreaId { get; set; }

        [JsonPropertyName("areaName")]
        public string AreaName { get; set; }

        [JsonPropertyName("ianaTimeZone")]
        public string IanaTimeZone { get; set; }

        [JsonPropertyName("adminDistrictCode")]
        public string AdminDistrictCode { get; set; }

        [JsonPropertyName("adminDistrict")]
        public string AdminDistrict { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("countryName")]
        public string CountryName { get; set; }

        [JsonPropertyName("headlineText")]
        public string HeadlineText { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("disclaimer")]
        public object Disclaimer { get; set; }

        [JsonPropertyName("issueTimeLocal")]
        public string? IssueTimeLocal { get; set; }

        [JsonPropertyName("issueTimeLocalTimeZone")]
        public string? IssueTimeLocalTimeZone { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("processTimeUTC")]
        public int ProcessTimeUTC { get; set; }

        [JsonPropertyName("endTimeLocal")]
        public string? EndTimeLocal { get; set; }

        [JsonPropertyName("endTimeLocalTimeZone")]
        public string? EndTimeLocalTimeZone { get; set; }

        [JsonPropertyName("endTimeUTC")]
        public int EndTimeUTC { get; set; }

        [JsonPropertyName("displayRank")]
        public int DisplayRank { get; set; }

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