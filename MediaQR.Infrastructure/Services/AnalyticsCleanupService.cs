using MediaQR.Infrastructure.Data;
using MediaQR.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaQR.Infrastructure.Services;

public class AnalyticsCleanupService : BackgroundService
{
    private const int BatchSize = 1000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AnalyticsPrivacyOptions _options;
    private readonly ILogger<AnalyticsCleanupService> _logger;

    public AnalyticsCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<AnalyticsPrivacyOptions> options,
        ILogger<AnalyticsCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var interval =
            TimeSpan.FromHours(
                Math.Max(
                    1,
                    _options.CleanupIntervalHours));

        // اجرای اولیه بعد از بالا آمدن برنامه
        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(
            stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var retentionDays =
                Math.Max(
                    1,
                    _options.RetentionDays);

            var cutoff =
                DateTime.UtcNow.AddDays(
                    -retentionDays);

            var totalDeleted = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<MediaDbContext>();

                var ids =
                    await dbContext.AnalyticsEvents
                        .Where(x => x.CreatedAt < cutoff)
                        .OrderBy(x => x.Id)
                        .Select(x => x.Id)
                        .Take(BatchSize)
                        .ToListAsync(cancellationToken);

                if (ids.Count == 0)
                    break;

                await dbContext.AnalyticsEvents
                    .Where(x => ids.Contains(x.Id))
                    .ExecuteDeleteAsync(
                        cancellationToken);

                totalDeleted += ids.Count;

                if (ids.Count < BatchSize)
                    break;
            }

            if (totalDeleted > 0)
            {
                _logger.LogInformation(
                    "Analytics cleanup completed. Deleted {Count} events older than {Cutoff}.",
                    totalDeleted,
                    cutoff);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Application is shutting down.
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred during analytics cleanup.");
        }
    }
}