using MediaQR.Application.DTOs.Analytics;

namespace MediaQR.Application.Interfaces;

public interface IAnalyticsReportService
{
    Task<AnalyticsSummaryDto> GetSummaryAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsTimelineItemDto>> GetTimelineAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetDeviceBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetOperatingSystemBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetBrowserBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetReferrerBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
    GetCountryBreakdownAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetCityBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default);

    Task<AnalyticsQrCodeSearchResultDto> 
        SearchQrCodesAsync(
            string? search = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
}