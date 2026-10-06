namespace MediaQR.Application.DTOs.Media;

public class CreateMediaResultDto
{
    public long MediaFileId { get; set; }

    public long QrCodeId { get; set; }

    public string Title { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public string QrCode { get; set; } = null!;

    public string QrUrl { get; set; } = null!;

    public string QrImageBase64 { get; set; } = null!;
}