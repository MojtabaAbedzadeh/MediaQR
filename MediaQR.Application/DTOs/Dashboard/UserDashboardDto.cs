namespace MediaQR.Application.DTOs.Dashboard;

public class UserDashboardDto
{
    public string UserName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public int MediaCount { get; set; }

    public int QrCodeCount { get; set; }

    public int PlayCount { get; set; }

    public IReadOnlyList<UserDashboardQrDto> RecentQrCodes { get; set; }
        = Array.Empty<UserDashboardQrDto>();
}

public class UserDashboardQrDto
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string MediaTitle { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired { get; set; }

    public string QrUrl { get; set; } = string.Empty;
}