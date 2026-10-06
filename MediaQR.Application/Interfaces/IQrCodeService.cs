namespace MediaQR.Application.Interfaces;

public interface IQrCodeService
{
    Task<byte[]> GenerateAsync(
        string content,
        CancellationToken cancellationToken = default);
}