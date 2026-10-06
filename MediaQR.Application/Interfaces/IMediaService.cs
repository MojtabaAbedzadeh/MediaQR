using MediaQR.Application.DTOs.Media;

namespace MediaQR.Application.Interfaces;

public interface IMediaService
{
    Task<CreateMediaResultDto> CreateAsync(
        CreateMediaDto dto,
        CancellationToken cancellationToken = default);
}