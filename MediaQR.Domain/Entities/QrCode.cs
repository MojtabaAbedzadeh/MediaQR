namespace MediaQR.Domain.Entities;

public class QrCode
{
    public long Id { get; set; }
    public long MediaFileId { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? OrderNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public MediaFile MediaFile { get; set; } = null!;
    public ICollection<AnalyticsEvent> AnalyticsEvents { get; set; } = new List<AnalyticsEvent>();
}