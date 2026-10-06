using MediaQR.Application.DTOs.Viewer;

namespace MediaQR.Application.Interfaces;

public interface IMediaViewerService
{
    Task<MediaViewerDto?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenContentAsync(
        string code,
        CancellationToken cancellationToken = default);
}