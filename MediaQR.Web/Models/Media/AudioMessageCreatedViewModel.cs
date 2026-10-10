namespace MediaQR.Web.Models.Media;

public class AudioMessageCreatedViewModel
{
    public long MediaFileId { get; set; }

    public long QrCodeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string QrUrl { get; set; } = string.Empty;

    public string QrImageUrl { get; set; } = string.Empty;

    public string ViewerUrl { get; set; } = string.Empty;
}