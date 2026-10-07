namespace MediaQR.Application.DTOs.Analytics;

public class AnalyticsTimelineItemDto
{
    public DateTime Date { get; set; }
    public long TotalScans { get; set; }
    public long TotalPlays { get; set; }
    public long TotalPauses { get; set; }
    public long Progress25Count { get; set; }
    public long Progress50Count { get; set; }
    public long Progress75Count { get; set; }
    public long Progress100Count { get; set; }
    public long CompletedPlays { get; set; }
    public long UniqueVisitors { get; set; }
}