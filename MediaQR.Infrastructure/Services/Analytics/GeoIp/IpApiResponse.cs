using System.Text.Json.Serialization;

namespace MediaQR.Infrastructure.Services.Analytics.GeoIp;

internal class IpApiResponse
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("is_bogon")]
    public bool IsBogon { get; set; }

    [JsonPropertyName("location")]
    public IpApiLocation? Location { get; set; }
}

internal class IpApiLocation
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
}