using MediaQR.Application.Interfaces;
using MediaQR.Application.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace MediaQR.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var configuredRootPath = options.Value.RootPath;

        if (string.IsNullOrWhiteSpace(configuredRootPath))
            throw new InvalidOperationException(
                "File storage root path is not configured.");

        _rootPath = Path.GetFullPath(
            Path.Combine(
                environment.ContentRootPath,
                configuredRootPath));
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
            throw new InvalidOperationException(
                "File extension is required.");

        extension = extension.ToLowerInvariant();

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;

        var relativeDirectory = Path.Combine(
            year.ToString(),
            month.ToString("00"));

        var physicalDirectory = Path.Combine(
            _rootPath,
            relativeDirectory);

        Directory.CreateDirectory(
            physicalDirectory);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath = Path.Combine(
            physicalDirectory,
            storedFileName);

        await using var outputStream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true);

        await fileStream.CopyToAsync(
            outputStream,
            cancellationToken);

        return Path.Combine(
                relativeDirectory,
                storedFileName)
            .Replace('\\', '/');
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return Task.CompletedTask;

        var fullPath = GetSafePhysicalPath(
            storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var fullPath = GetSafePhysicalPath(
            storagePath);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }

    private string GetSafePhysicalPath(
        string storagePath)
    {
        var normalizedPath = storagePath
            .Replace(
                '/',
                Path.DirectorySeparatorChar)
            .Replace(
                '\\',
                Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(
                _rootPath,
                normalizedPath));

        var rootWithSeparator =
            _rootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid storage path.");
        }

        return fullPath;
    }
}