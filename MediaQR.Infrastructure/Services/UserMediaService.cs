using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Enums;
using MediaQR.Infrastructure.Data;
using MediaQR.Infrastructure.Identity;
using MediaQR.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MediaQR.Infrastructure.Services;

public class UserMediaService : IUserMediaService
{
    private readonly MediaDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediaUrlSigner _mediaUrlSigner;
    private readonly MediaUrlSecurityOptions _mediaUrlSecurityOptions;
    private readonly IFileStorageService _fileStorageService;

    public UserMediaService(
     MediaDbContext dbContext,
     UserManager<ApplicationUser> userManager,
     IHttpContextAccessor httpContextAccessor,
     IMediaUrlSigner mediaUrlSigner,
     IOptions<MediaUrlSecurityOptions> mediaUrlSecurityOptions,
     IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _mediaUrlSigner = mediaUrlSigner;
        _mediaUrlSecurityOptions = mediaUrlSecurityOptions.Value;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<UserMediaListDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var query = _dbContext.MediaFiles
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserMediaListDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                MediaType = GetMediaTypeName(x.MediaType),
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                CreatedAt = x.CreatedAt,
                IsActive = x.IsActive,
                QrCodeCount = x.QrCodes.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserMediaDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var query = _dbContext.MediaFiles
            .AsNoTracking()
            .Where(x => x.Id == id);

        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var media = await query
            .Include(x => x.QrCodes)
            .FirstOrDefaultAsync(cancellationToken);

        if (media == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        // پیدا کردن یک QR فعال و معتبر برای Preview
        var previewQr = media.QrCodes
            .Where(x => x.IsActive)
            .Where(x =>
                !x.ExpiresAt.HasValue ||
                x.ExpiresAt.Value > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        return new UserMediaDetailsDto
        {
            Id = media.Id,
            Title = media.Title,
            Description = media.Description,
            MediaType = GetMediaTypeName(media.MediaType),
            OriginalFileName = media.OriginalFileName,
            ContentType = media.ContentType,
            FileSize = media.FileSize,
            CreatedAt = media.CreatedAt,
            IsActive = media.IsActive,

            QrCodeCount = media.QrCodes.Count,

            // اگر QR معتبر وجود داشته باشد، URL امضاشده ساخته می‌شود
            ContentUrl = previewQr == null
                ? null
                : BuildContentUrl(previewQr.Code),

            QrCodes = media.QrCodes
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new UserMediaQrDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Title = x.Title,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt,

                    IsExpired =
                        x.ExpiresAt.HasValue &&
                        x.ExpiresAt.Value <= now,

                    QrUrl = BuildQrUrl(x.Code)
                })
                .ToList()
        };
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

    private string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "برای مشاهده فایل‌های رسانه‌ای باید وارد حساب کاربری شوید.");
        }

        var userId = user.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException(
                "شناسه کاربر در اطلاعات احراز هویت یافت نشد.");
        }

        return userId;
    }

    private bool IsCurrentUserAdmin()
    {
        return _httpContextAccessor
            .HttpContext?
            .User
            .IsInRole("Admin") == true;
    }

    private static string GetMediaTypeName(
        MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.Image => "تصویر",
            MediaType.Audio => "صوت",
            MediaType.Video => "ویدئو",
            _ => "نامشخص"
        };
    }

    public async Task<bool> UpdateAsync(
    long id,
    UpdateMediaDto dto,
    CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var query = _dbContext.MediaFiles
            .Where(x => x.Id == id);

        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var media = await query
            .FirstOrDefaultAsync(cancellationToken);

        if (media == null)
            return false;

        var title = dto.Title?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException(
                "عنوان فایل الزامی است.");

        media.Title = title;

        media.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

        media.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string BuildContentUrl(string code)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request == null)
        {
            throw new InvalidOperationException(
                "HTTP Request در دسترس نیست.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(
            _mediaUrlSecurityOptions.LifetimeSeconds);

        var signature = _mediaUrlSigner.CreateSignature(
            code,
            expiresAt);

        var expires = expiresAt.ToUnixTimeSeconds();

        return
            $"{request.Scheme}://{request.Host}" +
            $"/media/{Uri.EscapeDataString(code)}/content" +
            $"?expires={expires}&signature={Uri.EscapeDataString(signature)}";
    }

    public async Task<bool> DeleteAsync(
    long id,
    CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var query = _dbContext.MediaFiles
            .Include(x => x.QrCodes)
                .ThenInclude(x => x.AnalyticsEvents)
            .Where(x => x.Id == id);

        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var media = await query
            .FirstOrDefaultAsync(cancellationToken);

        if (media == null)
        {
            return false;
        }

        var storagePath = media.StoragePath;

        _dbContext.MediaFiles.Remove(media);

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            if (!string.IsNullOrWhiteSpace(storagePath))
            {
                await _fileStorageService.DeleteAsync(
                    storagePath,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // حذف دیتابیس موفق بوده است.
            // خطای حذف فایل فیزیکی نباید باعث rollback
            // حذف رکوردهای دیتابیس شود.
            throw new InvalidOperationException(
                "اطلاعات فایل حذف شد، اما حذف فایل فیزیکی با خطا مواجه شد.",
                ex);
        }

        return true;
    }
}