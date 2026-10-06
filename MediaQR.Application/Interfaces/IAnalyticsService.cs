using MediaQR.Application.DTOs.Analytics;

namespace MediaQR.Application.Interfaces;

public interface IAnalyticsService
{
    Task TrackAsync(TrackAnalyticsEventDto dto, CancellationToken cancellationToken = default);
}