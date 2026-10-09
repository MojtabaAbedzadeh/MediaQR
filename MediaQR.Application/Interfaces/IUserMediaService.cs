using MediaQR.Application.DTOs.Media;

namespace MediaQR.Application.Interfaces;

public interface IUserMediaService
{
    Task<IReadOnlyList<UserMediaListDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<UserMediaDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        long id,
        UpdateMediaDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
    long id,
    CancellationToken cancellationToken = default);
}