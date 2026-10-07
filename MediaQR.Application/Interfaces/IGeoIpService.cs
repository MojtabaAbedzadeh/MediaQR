using MediaQR.Application.DTOs.Analytics;

namespace MediaQR.Application.Interfaces;

public interface IGeoIpService
{
    Task<GeoIpResultDto?> ResolveAsync(
        string? ipAddress,
        CancellationToken cancellationToken = default);
}