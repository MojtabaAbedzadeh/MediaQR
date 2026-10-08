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
        {
            throw new InvalidOperationException(
                "عنوان رسانه الزامی است.");
        }

        if (dto.File == null)
        {
            throw new InvalidOperationException(
                "انتخاب فایل الزامی است.");
        }

        var mediaType = _mediaFileValidator.Validate(
    dto.File.FileName,
    dto.File.ContentType,
    dto.File.Length,
    _fileValidationOptions);

        await using var validationStream =
            dto.File.OpenReadStream();

        var extension =
            Path.GetExtension(dto.File.FileName);

        await _fileSignatureValidator.ValidateAsync(
            validationStream,
            mediaType,
            extension,
            cancellationToken);

        string? storagePath = null;

        try
        {
            await using var uploadStream =
                dto.File.OpenReadStream();

            storagePath = await _fileStorageService.SaveAsync(
                uploadStream,
                dto.File.FileName,
                dto.File.ContentType,
                cancellationToken);

            var code = await GenerateUniqueCodeAsync(
                cancellationToken);

            var mediaFile = new MediaFile
            {
                Title = dto.Title.Trim(),
                UserId = userId,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                OriginalFileName = dto.File.FileName,
                StoredFileName = Path.GetFileName(storagePath),
                StoragePath = storagePath,
                ContentType = dto.File.ContentType,
                FileSize = dto.File.Length,
                MediaType =  mediaType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var qrCode = new QrCode
            {
                MediaFile = mediaFile,
                Code = code,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                OrderNumber = string.IsNullOrWhiteSpace(dto.OrderNumber) ? null : dto.OrderNumber.Trim(),
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateMediaResultDto
            {
                MediaFileId =
                    mediaFile.Id,

                QrCodeId =
                    qrCode.Id,

                Title =
                    mediaFile.Title,

                OriginalFileName =
                    mediaFile.OriginalFileName,

                MediaType =
                    mediaFile.MediaType.ToString(),

                QrCode =
                    qrCode.Code,

                QrUrl =
                    qrUrl,

                QrImageBase64 =
                    Convert.ToBase64String(
                        qrImageBytes)
            };
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(storagePath))
            {
                try
                {
                    await _fileStorageService.DeleteAsync(
                        storagePath,
                        CancellationToken.None);
                }
                catch
                {
                    // جلوگیری از مخفی شدن خطای اصلی
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