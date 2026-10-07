using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;

namespace MediaQR.Infrastructure.Services.Analytics.GeoIp;

public class NullGeoIpService : IGeoIpService
{
    public Task<GeoIpResultDto?> ResolveAsync(
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<GeoIpResultDto?>(null);
    }
}