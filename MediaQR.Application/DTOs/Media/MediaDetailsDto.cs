namespace MediaQR.Application.DTOs.Media;

public class MediaDetailsDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string MediaType { get; set; } = null!;
    public long FileSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<MediaQrItemDto> QrCodes { get; set; } = new List<MediaQrItemDto>();
}