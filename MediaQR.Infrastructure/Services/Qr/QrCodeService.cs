using MediaQR.Application.Interfaces;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace MediaQR.Infrastructure.Qr;

public class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateAsync(
        string content,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "QR content cannot be empty.",
                nameof(content));
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var qrGenerator = new QRCodeGenerator();

        using var qrData =
            qrGenerator.CreateQrCode(
                content,
                QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrData);

        var qrBytes =
            pngQrCode.GetGraphic(20);

        if (string.IsNullOrWhiteSpace(caption))
        {
            return Task.FromResult(qrBytes);
        }

        var result =
            AddCaptionBelowQr(
                qrBytes,
                caption.Trim());

        return Task.FromResult(result);
    }


    private static byte[] AddCaptionBelowQr(
        byte[] qrBytes,
        string caption)
    {
        using var qrStream =
            new MemoryStream(qrBytes);

        using var qrImage =
            Image.FromStream(qrStream);

        const int horizontalPadding = 30;
        const int captionHeight = 90;

        var outputWidth =
            qrImage.Width + (horizontalPadding * 2);

        var outputHeight =
            qrImage.Height + captionHeight;

        using var bitmap =
            new Bitmap(
                outputWidth,
                outputHeight);

        using (var graphics =
               Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);

            graphics.DrawImage(
                qrImage,
                horizontalPadding,
                0,
                qrImage.Width,
                qrImage.Height);

            using var font =
                new Font(
                    "Tahoma",
                    20,
                    FontStyle.Bold);

            using var brush =
                new SolidBrush(
                    Color.FromArgb(
                        80,
                        58,
                        86));

            using var format =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            var captionRectangle =
                new RectangleF(
                    0,
                    qrImage.Height,
                    outputWidth,
                    captionHeight);

            graphics.DrawString(
                $"سفارش: {caption}",
                font,
                brush,
                captionRectangle,
                format);
        }

        using var outputStream =
            new MemoryStream();

        bitmap.Save(
            outputStream,
            ImageFormat.Png);

        return outputStream.ToArray();
    }
}