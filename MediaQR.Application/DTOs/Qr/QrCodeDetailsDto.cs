namespace MediaQR.Application.DTOs.Qr;

public class QrCodeDetailsDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long MediaFileId { get; set; }
    public string MediaTitle { get; set; } = null!;
    public string? MediaDescription { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string MediaType { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public string QrUrl { get; set; } = null!;
    public string QrImageBase64 { get; set; } = null!;
}