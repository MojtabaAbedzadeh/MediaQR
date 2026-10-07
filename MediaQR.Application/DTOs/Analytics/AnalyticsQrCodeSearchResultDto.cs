namespace MediaQR.Application.DTOs.Analytics;

public class AnalyticsQrCodeSearchResultDto
{
    public IReadOnlyList<AnalyticsQrCodeItemDto> Items { get; set; } = Array.Empty<AnalyticsQrCodeItemDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}