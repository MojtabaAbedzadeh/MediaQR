using MediaQR.Application.DTOs.Viewer;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaQR.Infrastructure.Services;

public class MediaViewerService : IMediaViewerService
{
    private readonly MediaDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public MediaViewerService(
    MediaDbContext dbContext,
    IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<MediaViewerDto?> GetByCodeAsync(
    string code,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var now = DateTime.UtcNow;

        var qrCode = await _dbContext.QrCodes
            .AsNoTracking()
            .Include(x => x.MediaFile)
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);

        if (qrCode == null)
            return null;

        // QR غیرفعال است
        if (!qrCode.IsActive)
            return null;

        // QR منقضی شده است
        if (qrCode.ExpiresAt.HasValue &&
            qrCode.ExpiresAt.Value <= now)
        {
            return null;
        }

        var media = qrCode.MediaFile;

        if (media == null)
            return null;

        // فایل Media غیرفعال است
        if (!media.IsActive)
            return null;

        return new MediaViewerDto
        {
            MediaFileId = media.Id,

            QrCodeId = qrCode.Id,

            QrCode = qrCode.Code,

            ContentUrl = null,

            Title = string.IsNullOrWhiteSpace(qrCode.Title)
                ? media.Title
                : qrCode.Title,

            Description = string.IsNullOrWhiteSpace(qrCode.Description)
                ? media.Description
                : qrCode.Description,

            OriginalFileName = media.OriginalFileName,

            ContentType = media.ContentType,

            MediaType = media.MediaType.ToString(),

            StoragePath = media.StoragePath
        };
    }

    public async Task<Stream?> OpenContentAsync(
    string code,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        code = code.Trim();

        var qrCode = await _dbContext.QrCodes
            .AsNoTracking()
            .Include(x => x.MediaFile)
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);

        if (qrCode == null)
            return null;

        if (!qrCode.IsActive)
            return null;

        if (!qrCode.MediaFile.IsActive)
            return null;

        if (qrCode.ExpiresAt.HasValue &&
            qrCode.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return null;
        }

        return await _fileStorageService.OpenReadAsync(
            qrCode.MediaFile.StoragePath,
            cancellationToken);
    }
}