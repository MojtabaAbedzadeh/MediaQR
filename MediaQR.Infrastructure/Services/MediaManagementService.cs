using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using MediaQR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaQR.Infrastructure.Services;

public class MediaManagementService : IMediaManagementService
{
    private readonly MediaDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MediaManagementService> _logger;

    public MediaManagementService(
        MediaDbContext dbContext,
        IFileStorageService fileStorageService,
        ILogger<MediaManagementService> logger)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MediaListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var mediaFiles = await _dbContext.MediaFiles
            .AsNoTracking()
            .Include(x => x.QrCodes)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return mediaFiles
            .Select(x => new MediaListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                MediaType = x.MediaType.ToString(),
                FileSize = x.FileSize,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                QrCount = x.QrCodes.Count
            })
            .ToList();
    }

    public async Task<MediaDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var mediaFile = await _dbContext.MediaFiles
            .AsNoTracking()
            .Include(x => x.QrCodes)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (mediaFile == null)
            return null;

        return new MediaDetailsDto
        {
            Id = mediaFile.Id,
            Title = mediaFile.Title,
            Description = mediaFile.Description,
            OriginalFileName = mediaFile.OriginalFileName,
            ContentType = mediaFile.ContentType,
            MediaType = mediaFile.MediaType.ToString(),
            FileSize = mediaFile.FileSize,
            IsActive = mediaFile.IsActive,
            CreatedAt = mediaFile.CreatedAt,

            QrCodes = mediaFile.QrCodes
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new MediaQrItemDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Title = x.Title,
                    IsActive = x.IsActive,
                    ExpiresAt = x.ExpiresAt
                })
                .ToList()
        };
    }

    public async Task<bool> SetActiveAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var mediaFile = await _dbContext.MediaFiles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (mediaFile == null)
            return false;

        mediaFile.IsActive = isActive;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var mediaFile = await _dbContext.MediaFiles
            .Include(x => x.QrCodes)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (mediaFile == null)
            return false;

        if (mediaFile.QrCodes.Any())
        {
            throw new InvalidOperationException(
                "این رسانه دارای QR Code مرتبط است و ابتدا باید QR Codeهای آن حذف شوند.");
        }

        var storagePath = mediaFile.StoragePath;

        _dbContext.MediaFiles.Remove(mediaFile);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        try
        {
            await _fileStorageService.DeleteAsync(
                storagePath,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "رسانه از دیتابیس حذف شد اما حذف فایل فیزیکی ناموفق بود. StoragePath: {StoragePath}",
                storagePath);
        }

        return true;
    }
}