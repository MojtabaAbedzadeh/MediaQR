using MediaQR.Application.DTOs.Analytics;

namespace MediaQR.Application.Interfaces;

public interface IUserMediaAnalyticsService
{
    Task<UserMediaAnalyticsDto?> GetMediaAnalyticsAsync(
        long mediaFileId,
        CancellationToken cancellationToken = default);

    Task<UserQrAnalyticsDto?> GetQrAnalyticsAsync(
        long qrCodeId,
        CancellationToken cancellationToken = default);
}