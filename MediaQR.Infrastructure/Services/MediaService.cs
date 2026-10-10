using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using MediaQR.Application.Options;
using MediaQR.Application.Validators;
using MediaQR.Domain.Entities;
using MediaQR.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MediaQR.Infrastructure.Services;

public class MediaService : IMediaService
{
    private readonly MediaDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly MediaFileValidator _mediaFileValidator;
    private readonly FileSignatureValidator _fileSignatureValidator;
    private readonly IQrCodeService _qrCodeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileValidationOptions _fileValidationOptions;

    public MediaService(
     MediaDbContext dbContext,
     IFileStorageService fileStorageService,
     MediaFileValidator mediaFileValidator,
     FileSignatureValidator fileSignatureValidator,
     IQrCodeService qrCodeService,
     IHttpContextAccessor httpContextAccessor,
     IOptions<FileValidationOptions> fileValidationOptions)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _mediaFileValidator = mediaFileValidator;
        _fileSignatureValidator = fileSignatureValidator;
        _qrCodeService = qrCodeService;
        _httpContextAccessor = httpContextAccessor;
        _fileValidationOptions = fileValidationOptions.Value;
    }

    public async Task<CreateMediaResultDto> CreateAsync(
        CreateMediaDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new InvalidOperationException("عنوان رسانه الزامی است.");

        if (dto.File == null || dto.File.Length == 0)
            throw new InvalidOperationException("انتخاب فایل صوتی الزامی است.");

        // اعتبارسنجی فایل صوتی
        var mediaType = _mediaFileValidator.Validate(
            dto.File.FileName,
            dto.File.ContentType,
            dto.File.Length,
            _fileValidationOptions);

        await using (var validationStream = dto.File.OpenReadStream())
        {
            await _fileSignatureValidator.ValidateAsync(
                validationStream,
                mediaType,
                Path.GetExtension(dto.File.FileName),
                cancellationToken);
        }

        if (mediaType != MediaQR.Domain.Enums.MediaType.Audio)
        {
            throw new InvalidOperationException(
                "در این فرم فقط فایل صوتی مجاز است.");
        }

        var coverImage = dto.CoverImage;

        // اعتبارسنجی تصویر کاور
        if (coverImage != null)
        {
            if (coverImage.Length == 0)
                throw new InvalidOperationException("فایل تصویر خالی است.");

            var extension = Path.GetExtension(coverImage.FileName);

            var allowedExtensions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "فرمت تصویر باید JPG، PNG یا WEBP باشد.");
            }

            var imageType = _mediaFileValidator.Validate(
                coverImage.FileName,
                coverImage.ContentType,
                coverImage.Length,
                _fileValidationOptions);

            if (imageType != MediaQR.Domain.Enums.MediaType.Image)
            {
                throw new InvalidOperationException(
                    "فایل انتخاب‌شده یک تصویر معتبر نیست.");
            }

            await using var imageValidationStream =
                coverImage.OpenReadStream();

            await _fileSignatureValidator.ValidateAsync(
                imageValidationStream,
                imageType,
                extension,
                cancellationToken);
        }

        string? audioStoragePath = null;
        string? coverStoragePath = null;

        // شروع تراکنش قبل از ذخیره رکوردهای دیتابیس
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. ذخیره فایل صوتی در فضای ذخیره‌سازی
            await using (var audioStream = dto.File.OpenReadStream())
            {
                audioStoragePath = await _fileStorageService.SaveAsync(
                    audioStream,
                    dto.File.FileName,
                    dto.File.ContentType,
                    cancellationToken);
            }

            // 2. ذخیره اختیاری تصویر کاور
            MediaFile? coverMedia = null;

            if (coverImage != null)
            {
                await using var imageStream = coverImage.OpenReadStream();

                coverStoragePath = await _fileStorageService.SaveAsync(
                    imageStream,
                    coverImage.FileName,
                    coverImage.ContentType,
                    cancellationToken);

                coverMedia = new MediaFile
                {
                    Title = $"{dto.Title.Trim()} - تصویر همراه",
                    UserId = userId,
                    OriginalFileName = Path.GetFileName(coverImage.FileName),
                    StoredFileName = Path.GetFileName(coverStoragePath),
                    StoragePath = coverStoragePath,
                    ContentType = coverImage.ContentType,
                    FileSize = coverImage.Length,
                    MediaType = MediaQR.Domain.Enums.MediaType.Image,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.MediaFiles.Add(coverMedia);

                // برای دریافت شناسه کاور؛ همچنان داخل تراکنش است.
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 3. ساخت رسانه اصلی
            var code = await GenerateUniqueCodeAsync(cancellationToken);

            var mediaFile = new MediaFile
            {
                Title = dto.Title.Trim(),
                UserId = userId,
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                OriginalFileName = Path.GetFileName(dto.File.FileName),
                StoredFileName = Path.GetFileName(audioStoragePath),
                StoragePath = audioStoragePath,
                ContentType = dto.File.ContentType,
                FileSize = dto.File.Length,
                MediaType = mediaType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CoverImageId = coverMedia?.Id
            };

            // 4. ساخت QR مرتبط با فایل صوتی
            var qrCode = new QrCode
            {
                MediaFile = mediaFile,
                Code = code,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                    ? null
                    : dto.PhoneNumber.Trim(),
                OrderNumber = string.IsNullOrWhiteSpace(dto.OrderNumber)
                    ? null
                    : dto.OrderNumber.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var qrUrl = BuildQrUrl(code);

            var qrImageBytes = await _qrCodeService.GenerateAsync(
                qrUrl,
                dto.OrderNumber,
                cancellationToken);

            _dbContext.MediaFiles.Add(mediaFile);
            _dbContext.QrCodes.Add(qrCode);

            // 5. ذخیره رسانه اصلی و QR
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 6. نهایی کردن تمام تغییرات دیتابیس
            await transaction.CommitAsync(cancellationToken);

            return new CreateMediaResultDto
            {
                MediaFileId = mediaFile.Id,
                QrCodeId = qrCode.Id,
                Title = mediaFile.Title,
                OriginalFileName = mediaFile.OriginalFileName,
                MediaType = mediaFile.MediaType.ToString(),
                QrCode = qrCode.Code,
                QrUrl = qrUrl,
                QrImageBase64 = Convert.ToBase64String(qrImageBytes)
            };
        }
        catch
        {
            // لغو تمام تغییرات دیتابیس، از جمله رکورد کاور
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // خطای اصلی حفظ می‌شود.
            }

            // حذف فایل صوتی در صورت ذخیره شدن
            if (!string.IsNullOrWhiteSpace(audioStoragePath))
            {
                try
                {
                    await _fileStorageService.DeleteAsync(
                        audioStoragePath,
                        CancellationToken.None);
                }
                catch
                {
                    // خطای اصلی حفظ می‌شود.
                }
            }

            // حذف تصویر کاور در صورت ذخیره شدن
            if (!string.IsNullOrWhiteSpace(coverStoragePath))
            {
                try
                {
                    await _fileStorageService.DeleteAsync(
                        coverStoragePath,
                        CancellationToken.None);
                }
                catch
                {
                    // خطای اصلی حفظ می‌شود.
                }
            }

            throw;
        }
    }

    private async Task<string> GenerateUniqueCodeAsync(
        CancellationToken cancellationToken)
    {
        const int codeLength = 12;

        while (true)
        {
            var bytes =
                RandomNumberGenerator.GetBytes(9);

            var code =
                Convert.ToBase64String(bytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .ToUpperInvariant();

            if (code.Length > codeLength)
            {
                code = code[..codeLength];
            }

            var exists =
                await _dbContext.QrCodes
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Code == code,
                        cancellationToken);

            if (!exists)
                return code;
        }
    }

    private string BuildQrUrl(
        string code)
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
        var user =
            _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "برای ایجاد رسانه باید وارد حساب کاربری شوید.");
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
}