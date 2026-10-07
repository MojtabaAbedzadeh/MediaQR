namespace MediaQR.Application.DTOs.Analytics;

public class AnalyticsSummaryDto
{
    public long TotalScans { get; set; }
    public long UniqueVisitors { get; set; }
    public long TotalPlays { get; set; }
    public long TotalPauses { get; set; }
    public long Progress25Count { get; set; }
    public long Progress50Count { get; set; }
    public long Progress75Count { get; set; }
    public long Progress100Count { get; set; }
    public long CompletedPlays { get; set; }
    public double CompletionRate { get; set; }
    public double Progress25Rate { get; set; }
    public double Progress50Rate { get; set; }
    public double Progress75Rate { get; set; }
    public double Progress100Rate { get; set; }
    public double? AverageDurationSeconds { get; set; }
}