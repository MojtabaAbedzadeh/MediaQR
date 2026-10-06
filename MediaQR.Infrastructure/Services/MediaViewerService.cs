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

        return new MediaViewerDto
        {
            MediaFileId = qrCode.MediaFileId,
            QrCodeId = qrCode.Id,
            QrCode = qrCode.Code,
            Title = qrCode.Title ?? qrCode.MediaFile.Title,
            Description = qrCode.Description ?? qrCode.MediaFile.Description,
            OriginalFileName = qrCode.MediaFile.OriginalFileName,
            ContentType = qrCode.MediaFile.ContentType,
            MediaType = qrCode.MediaFile.MediaType.ToString(),
            StoragePath = qrCode.MediaFile.StoragePath
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