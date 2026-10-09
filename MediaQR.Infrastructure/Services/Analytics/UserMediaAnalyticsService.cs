using System.Security.Claims;
using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Domain.Enums;
using MediaQR.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediaQR.Infrastructure.Services.Analytics;

public sealed class UserMediaAnalyticsService : IUserMediaAnalyticsService
{
    private readonly MediaDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserMediaAnalyticsService(
        MediaDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserMediaAnalyticsDto?> GetMediaAnalyticsAsync(
        long mediaFileId,
        CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var isAdmin = user.IsInRole("Admin");

        // مالکیت از همان ابتدا در Query اعمال می‌شود.
        var mediaQuery = _dbContext.MediaFiles
            .AsNoTracking()
            .Where(x => x.Id == mediaFileId);

        if (!isAdmin)
            mediaQuery = mediaQuery.Where(x => x.UserId == userId);

        var media = await mediaQuery
            .Select(x => new
            {
                x.Id,
                x.Title
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (media == null)
            return null;

        var qrQuery = _dbContext.QrCodes
            .AsNoTracking()
            .Where(x => x.MediaFileId == mediaFileId);

        var qrCodes = await qrQuery
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Title,
                x.IsActive,
                x.ExpiresAt,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var eventsQuery = _dbContext.AnalyticsEvents
            .AsNoTracking()
            .Where(x => x.QrCode.MediaFileId == mediaFileId);

        var summary = await eventsQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                ScanCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Scan),

                PlayCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Play),

                PauseCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Pause),

                Progress25Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress25),

                Progress50Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress50),

                Progress75Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress75),

                Progress100Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress100),

                CompleteCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Complete),

                UniqueVisitorCount = g
                    .Where(x =>
                        x.VisitorId != null &&
                        x.VisitorId != "")
                    .Select(x => x.VisitorId!)
                    .Distinct()
                    .LongCount(),

                AverageDurationSeconds = g
                    .Where(x =>
                        x.DurationSeconds.HasValue &&
                        x.DurationSeconds.Value >= 0)
                    .Select(x => x.DurationSeconds)
                    .Average()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var lastActivity = await eventsQuery
            .Select(x => (DateTime?)x.CreatedAt)
            .MaxAsync(cancellationToken);

        // آمار هر QR با یک Query گروه‌بندی‌شده دریافت می‌شود.
        var qrStats = await eventsQuery
            .GroupBy(x => x.QrCodeId)
            .Select(g => new
            {
                QrCodeId = g.Key,

                ScanCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Scan),

                PlayCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Play),

                PauseCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Pause),

                CompleteCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Complete),

                LastActivityAt = g.Max(x => x.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        var statsByQrId = qrStats.ToDictionary(x => x.QrCodeId);

        var qrAnalytics = qrCodes
            .Select(qr =>
            {
                statsByQrId.TryGetValue(qr.Id, out var stats);

                return new UserQrAnalyticsDto
                {
                    QrCodeId = qr.Id,
                    Code = qr.Code,
                    Title = qr.Title,
                    IsActive = qr.IsActive,
                    ExpiresAt = qr.ExpiresAt,

                    ScanCount = stats?.ScanCount ?? 0,
                    PlayCount = stats?.PlayCount ?? 0,
                    PauseCount = stats?.PauseCount ?? 0,
                    CompleteCount = stats?.CompleteCount ?? 0,
                    LastActivityAt = stats?.LastActivityAt
                };
            })
            .ToList();

        // اطلاعات رویدادها را فقط پس از اعمال فیلتر مالکیت می‌خوانیم.
        var recentRows = await eventsQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.QrCodeId,
                QrCode = x.QrCode.Code,
                QrTitle = x.QrCode.Title,
                x.EventType,
                x.CreatedAt,
                x.DeviceType,
                x.OperatingSystem,
                x.Browser,
                x.Country,
                x.City
            })
            .ToListAsync(cancellationToken);

        var recentEvents = recentRows
            .Select(x => new UserAnalyticsEventDto
            {
                Id = x.Id,
                QrCodeId = x.QrCodeId,
                QrCode = x.QrCode,
                QrTitle = x.QrTitle,
                EventType = x.EventType.ToString(),
                CreatedAt = x.CreatedAt,
                DeviceType = x.DeviceType,
                OperatingSystem = x.OperatingSystem,
                Browser = x.Browser,
                Country = x.Country,
                City = x.City
            })
            .ToList();

        return new UserMediaAnalyticsDto
        {
            MediaFileId = media.Id,
            MediaTitle = media.Title,
            QrCodeCount = qrCodes.Count,

            ScanCount = summary?.ScanCount ?? 0,
            PlayCount = summary?.PlayCount ?? 0,
            PauseCount = summary?.PauseCount ?? 0,

            Progress25Count = summary?.Progress25Count ?? 0,
            Progress50Count = summary?.Progress50Count ?? 0,
            Progress75Count = summary?.Progress75Count ?? 0,
            Progress100Count = summary?.Progress100Count ?? 0,

            CompleteCount = summary?.CompleteCount ?? 0,
            UniqueVisitorCount = summary?.UniqueVisitorCount ?? 0,
            AverageDurationSeconds = summary?.AverageDurationSeconds,
            LastActivityAt = lastActivity,

            QrCodes = qrAnalytics,
            RecentEvents = recentEvents
        };
    }

    public async Task<UserQrAnalyticsDto?> GetQrAnalyticsAsync(
        long qrCodeId,
        CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var isAdmin = user.IsInRole("Admin");

        // QR و مالک فایل در یک Query بررسی می‌شوند.
        var qrQuery = _dbContext.QrCodes
            .AsNoTracking()
            .Where(x => x.Id == qrCodeId);

        if (!isAdmin)
            qrQuery = qrQuery.Where(x => x.MediaFile.UserId == userId);

        var qr = await qrQuery
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Title,
                x.IsActive,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (qr == null)
            return null;

        var eventsQuery = _dbContext.AnalyticsEvents
            .AsNoTracking()
            .Where(x => x.QrCodeId == qrCodeId);

        var stats = await eventsQuery
            .GroupBy(x => x.QrCodeId)
            .Select(g => new
            {
                ScanCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Scan),

                PlayCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Play),

                PauseCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Pause),

                CompleteCount = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Complete),

                LastActivityAt = g.Max(x => x.CreatedAt)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new UserQrAnalyticsDto
        {
            QrCodeId = qr.Id,
            Code = qr.Code,
            Title = qr.Title,
            IsActive = qr.IsActive,
            ExpiresAt = qr.ExpiresAt,

            ScanCount = stats?.ScanCount ?? 0,
            PlayCount = stats?.PlayCount ?? 0,
            PauseCount = stats?.PauseCount ?? 0,
            CompleteCount = stats?.CompleteCount ?? 0,
            LastActivityAt = stats?.LastActivityAt
        };
    }
}