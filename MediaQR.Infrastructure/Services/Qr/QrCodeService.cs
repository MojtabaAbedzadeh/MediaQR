using MediaQR.Application.Interfaces;
using QRCoder;

namespace MediaQR.Infrastructure.Qr;

public class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "QR content cannot be empty.",
                nameof(content));
        }

        using var qrGenerator = new QRCodeGenerator();

        using var qrData =
            qrGenerator.CreateQrCode(
                content,
                QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrData);

        var pngBytes = pngQrCode.GetGraphic(20);

        return Task.FromResult(pngBytes);
    }
}