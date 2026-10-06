namespace MediaQR.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}