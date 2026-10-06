using MediaQR.Application.DTOs.Qr;

namespace MediaQR.Application.Interfaces;

public interface IQrManagementService
{
    Task<IReadOnlyList<QrCodeListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<QrCodeDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        long id,
        UpdateQrCodeDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);
}