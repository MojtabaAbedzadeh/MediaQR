using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaQR.Infrastructure.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly MediaDbContext _dbContext;
    private readonly IRequestContextService _requestContextService;
    private readonly IUserAgentParser _userAgentParser;
    private readonly IGeoIpService _geoIpService;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
     MediaDbContext dbContext,
     IRequestContextService requestContextService,
     IUserAgentParser userAgentParser,
     IGeoIpService geoIpService,
     ILogger<AnalyticsService> logger)
    {
        _dbContext = dbContext;
        _requestContextService = requestContextService;
        _userAgentParser = userAgentParser;
        _geoIpService = geoIpService;
        _logger = logger;
    }

    public async Task TrackAsync(TrackAnalyticsEventDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.QrCodeId <= 0)
            throw new ArgumentException(
                "QR Code شناسه معتبری ندارد.",
                nameof(dto.QrCodeId));

        try
        {
            var qrExists = await _dbContext.QrCodes.AnyAsync(x => x.Id == dto.QrCodeId, cancellationToken);

            if (!qrExists)
            {
                _logger.LogWarning(
                    "تلاش برای ثبت Analytics برای QR Code نامعتبر. QrCodeId: {QrCodeId}",
                    dto.QrCodeId);

                throw new ArgumentException(
                    "QR Code مورد نظر وجود ندارد.",
                    nameof(dto.QrCodeId));
            }

            var userAgent = _requestContextService.GetUserAgent();

            var userAgentInfo = _userAgentParser.Parse(userAgent);

            var ipAddress = _requestContextService.GetIpAddress();

            GeoIpResultDto? geoIpResult = null;

            if (dto.EventType == AnalyticsEventType.Scan)
            {
                try
                {
                    geoIpResult =
                        await _geoIpService.ResolveAsync(
                            ipAddress,
                            cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "خطا هنگام Resolve کردن GeoIP. IP: {IpAddress}",
                        ipAddress);
                }
            }

            var analyticsEvent = new Domain.Entities.AnalyticsEvent
            {
                QrCodeId = dto.QrCodeId,
                EventType = dto.EventType,
                CreatedAt = DateTime.UtcNow,

                VisitorId = Normalize(_requestContextService.GetOrCreateVisitorId(), 100),
                IpAddress = Normalize(_requestContextService.GetIpAddress(), 100),
                UserAgent = Normalize(_requestContextService.GetUserAgent(), 1000),
                Referrer = Normalize(_requestContextService.GetReferrer(), 2000),

                DeviceType = Normalize(userAgentInfo.DeviceType, 50),
                OperatingSystem = Normalize(userAgentInfo.OperatingSystem, 100),
                Browser = Normalize(userAgentInfo.Browser, 100),
                BrowserVersion = Normalize(userAgentInfo.BrowserVersion, 50),

                Country = Normalize(geoIpResult?.Country, 100),

                City = Normalize(geoIpResult?.City, 100),

                DurationSeconds = NormalizeDuration(
                    dto.DurationSeconds),

                ProgressPercent = NormalizeProgress(
                    dto.ProgressPercent)
            };

            _dbContext.AnalyticsEvents.Add(
                analyticsEvent);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام ثبت Analytics Event. QrCodeId: {QrCodeId}, EventType: {EventType}",
                dto.QrCodeId,
                dto.EventType);

            throw;
        }
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();

        if (value.Length > maxLength)
            value = value[..maxLength];

        return value;
    }

    private static double? NormalizeDuration(
        double? duration)
    {
        if (!duration.HasValue)
            return null;

        if (double.IsNaN(duration.Value) ||
            double.IsInfinity(duration.Value))
        {
            return null;
        }

        if (duration.Value < 0)
            return null;

        return duration.Value;
    }

    private static double? NormalizeProgress(
        double? progress)
    {
        if (!progress.HasValue)
            return null;

        if (double.IsNaN(progress.Value) ||
            double.IsInfinity(progress.Value))
        {
            return null;
        }

        if (progress.Value < 0 ||
            progress.Value > 100)
        {
            return null;
        }

        return progress.Value;
    }
}