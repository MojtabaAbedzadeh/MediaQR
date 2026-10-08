using MediaQR.Application.DTOs.Dashboard;
using MediaQR.Application.Interfaces;
using MediaQR.Infrastructure.Data;
using MediaQR.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MediaQR.Domain.Enums;

namespace MediaQR.Infrastructure.Services;

public class UserDashboardService : IUserDashboardService
{
    private readonly MediaDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserDashboardService(
        MediaDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDashboardDto> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "کاربر جاری یافت نشد.");
        }

        var mediaQuery = _dbContext.MediaFiles
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var qrQuery = _dbContext.QrCodes
            .AsNoTracking()
            .Where(x => x.MediaFile.UserId == userId);

        var mediaCountTask =
            mediaQuery.CountAsync(cancellationToken);

        var qrCodeCountTask =
            qrQuery.CountAsync(cancellationToken);

        var playCountTask =
            qrQuery
                .SelectMany(x => x.AnalyticsEvents)
                .CountAsync(
                    x => x.EventType == AnalyticsEventType.Play,
                    cancellationToken);

        var recentQrCodesTask =
            qrQuery
                .Include(x => x.MediaFile)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

        await Task.WhenAll(
            mediaCountTask,
            qrCodeCountTask,
            playCountTask,
            recentQrCodesTask);

        var now = DateTime.UtcNow;

        var recentQrCodes =
            recentQrCodesTask.Result
                .Select(x => new UserDashboardQrDto
                {
                    Id = x.Id,

                    Code = x.Code,

                    Title = x.Title,

                    MediaTitle =
                        x.MediaFile.Title,

                    MediaType =
                        x.MediaFile.MediaType.ToString(),

                    IsActive =
                        x.IsActive,

                    CreatedAt =
                        x.CreatedAt,

                    ExpiresAt =
                        x.ExpiresAt,

                    IsExpired =
                        x.ExpiresAt.HasValue &&
                        x.ExpiresAt.Value <= now,

                    QrUrl =
                        BuildQrUrl(x.Code)
                })
                .ToList();

        return new UserDashboardDto
        {
            UserName =
                user.UserName ?? string.Empty,

            PhoneNumber =
                user.PhoneNumber,

            MediaCount =
                mediaCountTask.Result,

            QrCodeCount =
                qrCodeCountTask.Result,

            PlayCount =
                playCountTask.Result,

            RecentQrCodes =
                recentQrCodes
        };
    }

    private string GetCurrentUserId()
    {
        var user =
            _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "برای مشاهده داشبورد باید وارد حساب کاربری شوید.");
        }

        var userId =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException(
                "شناسه کاربر در اطلاعات احراز هویت یافت نشد.");
        }

        return userId;
    }

    private string BuildQrUrl(string code)
    {
        var request =
            _httpContextAccessor
                .HttpContext?
                .Request;

        if (request == null)
        {
            throw new InvalidOperationException(
                "HTTP Request در دسترس نیست.");
        }

        return $"{request.Scheme}://{request.Host}/m/{code}";
    }
}