namespace MediaQR.Infrastructure.Options;

public class GeoIpOptions
{
    public string Provider { get; set; } = "IpApi";
    public string BaseUrl { get; set; } ="https://api.ipapi.is/";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
    public int CacheMinutes { get; set; } = 1440;
}