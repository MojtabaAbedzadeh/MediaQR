namespace MediaQR.Application.DTOs.Viewer;

public class MediaViewerDto
{
    public long MediaFileId { get; set; }
    public long QrCodeId { get; set; }
    public string QrCode { get; set; } = null!;
    public string? ContentUrl { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string MediaType { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
}