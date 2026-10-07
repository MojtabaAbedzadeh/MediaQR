namespace MediaQR.Application.DTOs.Analytics;

public class AnalyticsQrCodeItemDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string MediaTitle { get; set; } = null!;
}