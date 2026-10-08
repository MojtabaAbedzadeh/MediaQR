using MediaQR.Application.DTOs.Qr;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediaQR.Infrastructure.Services;

public class QrManagementService : IQrManagementService
{
    private readonly MediaDbContext _dbContext;
    private readonly IQrCodeService _qrCodeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QrManagementService(
        MediaDbContext dbContext,
        IQrCodeService qrCodeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _qrCodeService = qrCodeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<QrCodeListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var qrCodes = await _dbContext.QrCodes
            .AsNoTracking()
            .Include(x => x.MediaFile)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return qrCodes
            .Select(x => new QrCodeListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                MediaFileId = x.MediaFileId,
                MediaTitle = x.MediaFile.Title,
                OriginalFileName = x.MediaFile.OriginalFileName,
                MediaType = x.MediaFile.MediaType.ToString(),
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
                IsExpired =
                    x.ExpiresAt.HasValue &&
                    x.ExpiresAt.Value <= now,
                QrUrl = BuildQrUrl(x.Code)
            })
            .ToList();
    }

    public async Task<QrCodeDetailsDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var qrCode = await _dbContext.QrCodes
            .AsNoTracking()
            .Include(x => x.MediaFile)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (qrCode == null)
            return null;

        var now = DateTime.UtcNow;

        var qrUrl = BuildQrUrl(qrCode.Code);

        var qrImageBytes = await _qrCodeService.GenerateAsync(
            qrUrl,
            qrCode.OrderNumber,
            cancellationToken);

        return new QrCodeDetailsDto
        {
            Id = qrCode.Id,
            Code = qrCode.Code,
            Title = qrCode.Title,
            Description = qrCode.Description,
            MediaFileId = qrCode.MediaFileId,
            MediaTitle = qrCode.MediaFile.Title,
            MediaDescription = qrCode.MediaFile.Description,
            OriginalFileName =
                qrCode.MediaFile.OriginalFileName,
            MediaType =
                qrCode.MediaFile.MediaType.ToString(),
            ContentType =
                qrCode.MediaFile.ContentType,
            FileSize =
                qrCode.MediaFile.FileSize,
            IsActive =
                qrCode.IsActive,
            CreatedAt =
                qrCode.CreatedAt,
            ExpiresAt =
                qrCode.ExpiresAt,
            IsExpired =
                qrCode.ExpiresAt.HasValue &&
                qrCode.ExpiresAt.Value <= now,
            QrUrl = qrUrl,
            QrImageBase64 =
                Convert.ToBase64String(qrImageBytes)
        };
    }

    public async Task<bool> UpdateAsync(
        long id,
        UpdateQrCodeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var qrCode = await _dbContext.QrCodes
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (qrCode == null)
            return false;

        qrCode.Title =
            string.IsNullOrWhiteSpace(dto.Title)
                ? null
                : dto.Title.Trim();

        qrCode.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

        qrCode.ExpiresAt = dto.ExpiresAt;
        qrCode.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> SetActiveAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var qrCode = await _dbContext.QrCodes
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (qrCode == null)
            return false;

        qrCode.IsActive = isActive;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var qrCode = await _dbContext.QrCodes
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (qrCode == null)
            return false;

        _dbContext.QrCodes.Remove(qrCode);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
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