namespace MediaQR.Application.DTOs.Qr;

public class UpdateQrCodeDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}