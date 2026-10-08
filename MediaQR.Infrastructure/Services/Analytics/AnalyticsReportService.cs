using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Domain.Enums;
using MediaQR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaQR.Infrastructure.Services.Analytics;

public class AnalyticsReportService : IAnalyticsReportService
{
    private readonly MediaDbContext _dbContext;

    public AnalyticsReportService(MediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AnalyticsEvent> query =
            _dbContext.AnalyticsEvents.AsNoTracking();

        if (qrCodeId.HasValue)
        {
            query = query.Where(x =>
                x.QrCodeId == qrCodeId.Value);
        }

        if (dateRange?.From.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt >= dateRange.From.Value);
        }

        if (dateRange?.To.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt < dateRange.To.Value);
        }

        var summary = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalScans = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Scan),

                UniqueVisitors = g
                    .Where(x =>
                        x.VisitorId != null &&
                        x.VisitorId != "")
                    .Select(x => x.VisitorId!)
                    .Distinct()
                    .LongCount(),

                TotalPlays = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Play),

                TotalPauses = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Pause),

                Progress25Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress25),

                Progress50Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress50),

                Progress75Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress75),

                Progress100Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress100),

                CompletedPlays = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Complete),

                AverageDurationSeconds =
                    g.Where(x =>
                            x.DurationSeconds.HasValue &&
                            x.DurationSeconds.Value >= 0)
                     .Select(x => x.DurationSeconds)
                     .Average()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary == null)
        {
            return new AnalyticsSummaryDto();
        }

        var totalPlays = summary.TotalPlays;

        return new AnalyticsSummaryDto
        {
            TotalScans = summary.TotalScans,

            UniqueVisitors = summary.UniqueVisitors,

            TotalPlays = summary.TotalPlays,

            TotalPauses = summary.TotalPauses,

            Progress25Count = summary.Progress25Count,

            Progress50Count = summary.Progress50Count,

            Progress75Count = summary.Progress75Count,

            Progress100Count = summary.Progress100Count,

            CompletedPlays = summary.CompletedPlays,

            CompletionRate =
                CalculateRate(
                    summary.CompletedPlays,
                    totalPlays),

            Progress25Rate =
                CalculateRate(
                    summary.Progress25Count,
                    totalPlays),

            Progress50Rate =
                CalculateRate(
                    summary.Progress50Count,
                    totalPlays),

            Progress75Rate =
                CalculateRate(
                    summary.Progress75Count,
                    totalPlays),

            Progress100Rate =
                CalculateRate(
                    summary.Progress100Count,
                    totalPlays),

            AverageDurationSeconds =
                summary.AverageDurationSeconds
        };
    }

    public async Task<IReadOnlyList<AnalyticsTimelineItemDto>> GetTimelineAsync(
    long? qrCodeId = null,
    AnalyticsDateRangeDto? dateRange = null,
    CancellationToken cancellationToken = default)
    {
        IQueryable<AnalyticsEvent> query =
            _dbContext.AnalyticsEvents
                .AsNoTracking();

        if (qrCodeId.HasValue)
        {
            query = query.Where(x =>
                x.QrCodeId == qrCodeId.Value);
        }

        if (dateRange?.From.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt >= dateRange.From.Value);
        }

        if (dateRange?.To.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt < dateRange.To.Value);
        }

        var result = await query
            .GroupBy(x => x.CreatedAt.Date)
            .Select(g => new AnalyticsTimelineItemDto
            {
                Date = g.Key,

                TotalScans = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Scan),

                TotalPlays = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Play),

                TotalPauses = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Pause),

                Progress25Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress25),

                Progress50Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress50),

                Progress75Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress75),

                Progress100Count = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Progress100),

                CompletedPlays = g.LongCount(
                    x => x.EventType == AnalyticsEventType.Complete),

                UniqueVisitors = g
                    .Where(x =>
                        x.VisitorId != null &&
                        x.VisitorId != "")
                    .Select(x => x.VisitorId!)
                    .Distinct()
                    .LongCount()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
    GetDeviceBreakdownAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.DeviceType)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return BuildBreakdown(
            rows.Select(x => (x.Name, x.Count)));
    }


    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetOperatingSystemBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.OperatingSystem)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return BuildBreakdown(
            rows.Select(x => (x.Name, x.Count)));
    }


    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
        GetBrowserBreakdownAsync(
            long? qrCodeId = null,
            AnalyticsDateRangeDto? dateRange = null,
            CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.Browser)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return BuildBreakdown(
            rows.Select(x => (x.Name, x.Count)));
    }

    private static double CalculateRate(
        long count,
        long total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Round(
            (double)count / total * 100,
            2);
    }

    private IQueryable<AnalyticsEvent> BuildScanQuery(
    long? qrCodeId,
    AnalyticsDateRangeDto? dateRange)
    {
        IQueryable<AnalyticsEvent> query =
            _dbContext.AnalyticsEvents
                .AsNoTracking()
                .Where(x =>
                    x.EventType == AnalyticsEventType.Scan);

        if (qrCodeId.HasValue)
        {
            query = query.Where(x =>
                x.QrCodeId == qrCodeId.Value);
        }

        if (dateRange?.From.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt >= dateRange.From.Value);
        }

        if (dateRange?.To.HasValue == true)
        {
            query = query.Where(x =>
                x.CreatedAt < dateRange.To.Value);
        }

        return query;
    }

    private static IReadOnlyList<AnalyticsBreakdownItemDto>
        BuildBreakdown(
            IEnumerable<(string? Name, long Count)> rows)
    {
        var list = rows
            .Select(x => new
            {
                Name = string.IsNullOrWhiteSpace(x.Name)
                    ? "Unknown"
                    : x.Name,
                x.Count
            })
            .ToList();

        var total = list.Sum(x => x.Count);

        return list
            .Select(x => new AnalyticsBreakdownItemDto
            {
                Name = x.Name,
                Count = x.Count,
                Percentage = total <= 0
                    ? 0
                    : Math.Round(
                        (double)x.Count / total * 100,
                        2)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
    GetReferrerBreakdownAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.Referrer)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var normalizedRows = rows.Select(x =>
            (
                Name: NormalizeReferrer(x.Name),
                Count: x.Count
            ));

        return BuildBreakdown(normalizedRows);
    }

    private static string NormalizeReferrer(string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer))
        {
            return "Direct";
        }

        return referrer.Trim();
    }

    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
    GetCountryBreakdownAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.Country)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var normalizedRows = rows.Select(x =>
            (
                Name: NormalizeLocation(x.Name),
                Count: x.Count
            ));

        return BuildBreakdown(normalizedRows);
    }

    public async Task<IReadOnlyList<AnalyticsBreakdownItemDto>>
    GetCityBreakdownAsync(
        long? qrCodeId = null,
        AnalyticsDateRangeDto? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildScanQuery(
            qrCodeId,
            dateRange);

        var rows = await query
            .GroupBy(x => x.City)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var normalizedRows = rows.Select(x =>
            (
                Name: NormalizeLocation(x.Name),
                Count: x.Count
            ));

        return BuildBreakdown(normalizedRows);
    }

    private static string NormalizeLocation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        return value.Trim();
    }

    public async Task<AnalyticsQrCodeSearchResultDto>
    SearchQrCodesAsync(
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        pageSize = Math.Clamp(
            pageSize,
            1,
            50);

        search = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        var query = _dbContext.QrCodes
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Code.StartsWith(search) ||
                (x.Title != null &&
                 x.Title.Contains(search)) ||
                x.MediaFile.Title.Contains(search));
        }

        var totalCount = await query
            .CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Title)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AnalyticsQrCodeItemDto
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                MediaTitle = x.MediaFile.Title
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        return new AnalyticsQrCodeSearchResultDto
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}