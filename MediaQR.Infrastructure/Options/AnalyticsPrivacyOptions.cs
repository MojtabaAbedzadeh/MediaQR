namespace MediaQR.Infrastructure.Options;

public class AnalyticsPrivacyOptions
{
    public int RetentionDays { get; set; } = 365;

    public int CleanupIntervalHours { get; set; } = 24;
}