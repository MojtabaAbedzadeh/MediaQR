namespace MediaQR.Application.DTOs.Qr;

public class QrCodeListItemDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public long MediaFileId { get; set; }
    public string MediaTitle { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string MediaType { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public string QrUrl { get; set; } = null!;
}