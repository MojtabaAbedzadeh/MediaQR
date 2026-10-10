using MediaQR.Domain.Enums;

namespace MediaQR.Domain.Entities;

public class MediaFile
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? UserId { get; set; }
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public MediaType MediaType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CoverImageId { get; set; }
    public MediaFile? CoverImage { get; set; }
    // ارتباط با QR Code ها
    public ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
}