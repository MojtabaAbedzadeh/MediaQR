namespace MediaQR.Application.DTOs.Analytics;

public class UserQrAnalyticsDto
{
    public long QrCodeId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Title { get; set; }

    public bool IsActive { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public long ScanCount { get; set; }

    public long PlayCount { get; set; }

    public long PauseCount { get; set; }

    public long CompleteCount { get; set; }

    public DateTime? LastActivityAt { get; set; }
}