namespace MediaQR.Application.DTOs.Media;

public class MediaQrItemDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
}