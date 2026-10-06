namespace MediaQR.Domain.Entities;

public class AnalyticsEvent
{
    public long Id { get; set; }
    public long QrCodeId { get; set; }
    public AnalyticsEventType EventType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? VisitorId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Browser { get; set; }
    public string? BrowserVersion { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Referrer { get; set; }
    public double? DurationSeconds { get; set; }
    public double? ProgressPercent { get; set; }
    public QrCode QrCode { get; set; } = null!;
}