namespace MediaQR.Application.DTOs.Analytics;

public class UserAnalyticsEventDto
{
    public long Id { get; set; }

    public long QrCodeId { get; set; }

    public string QrCode { get; set; } = string.Empty;

    public string? QrTitle { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? DeviceType { get; set; }

    public string? OperatingSystem { get; set; }

    public string? Browser { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }
}