namespace MediaQR.Application.Interfaces;

public interface IQrCodeService
{
    Task<byte[]> GenerateAsync(
        string content,
        string? caption = null,
        CancellationToken cancellationToken = default);
}