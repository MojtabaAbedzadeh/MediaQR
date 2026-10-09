namespace MediaQR.Application.DTOs.Analytics;

public class UserMediaAnalyticsDto
{
    public long MediaFileId { get; set; }

    public string MediaTitle { get; set; } = string.Empty;

    public int QrCodeCount { get; set; }

    public long ScanCount { get; set; }

    public long PlayCount { get; set; }

    public long PauseCount { get; set; }

    public long Progress25Count { get; set; }

    public long Progress50Count { get; set; }

    public long Progress75Count { get; set; }

    public long Progress100Count { get; set; }

    public long CompleteCount { get; set; }

    public DateTime? LastActivityAt { get; set; }
    public long UniqueVisitorCount { get; set; }

    public double? AverageDurationSeconds { get; set; }

    public IReadOnlyList<UserQrAnalyticsDto> QrCodes { get; set; }
        = Array.Empty<UserQrAnalyticsDto>();

    public IReadOnlyList<UserAnalyticsEventDto> RecentEvents { get; set; }
        = Array.Empty<UserAnalyticsEventDto>();
}