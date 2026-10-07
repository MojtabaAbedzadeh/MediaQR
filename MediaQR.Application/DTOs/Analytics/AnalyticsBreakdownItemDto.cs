namespace MediaQR.Application.DTOs.Analytics;

public class AnalyticsBreakdownItemDto
{
    public string Name { get; set; } = null!;
    public long Count { get; set; }
    public double Percentage { get; set; }
}