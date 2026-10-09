namespace MediaQR.Application.DTOs.Media;

public class UserMediaDetailsDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int QrCodeCount { get; set; }

    public string? ContentUrl { get; set; }

    public IReadOnlyList<UserMediaQrDto> QrCodes { get; set; }
        = Array.Empty<UserMediaQrDto>();
}

public class UserMediaQrDto
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Title { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired { get; set; }

    public string QrUrl { get; set; } = string.Empty;
}