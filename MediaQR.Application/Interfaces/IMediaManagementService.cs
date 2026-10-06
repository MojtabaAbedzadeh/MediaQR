using MediaQR.Application.DTOs.Media;

namespace MediaQR.Application.Interfaces;

public interface IMediaManagementService
{
    Task<IReadOnlyList<MediaListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<MediaDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);
}