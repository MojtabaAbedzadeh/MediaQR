using MediaQR.Domain.Entities;
using MediaQR.Domain.Enums;

namespace MediaQR.Application.Validators;

public class FileSignatureValidator
{
    public async Task ValidateAsync(
        Stream stream,
        MediaType mediaType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanRead)
            throw new InvalidOperationException(
                "امکان خواندن فایل وجود ندارد.");

        if (!stream.CanSeek)
            throw new InvalidOperationException(
                "Stream فایل قابل Seek نیست.");

        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException(
                "پسوند فایل مشخص نشده است.",
                nameof(extension));

        extension = extension.Trim().ToLowerInvariant();

        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        var originalPosition = stream.Position;

        try
        {
            stream.Position = 0;

            switch (mediaType)
            {
                case MediaType.Image:

                    await ValidateImageAsync(
                        stream,
                        extension,
                        cancellationToken);

                    break;

                case MediaType.Audio:

                    await ValidateAudioAsync(
                        stream,
                        extension,
                        cancellationToken);

                    break;

                case MediaType.Video:

                    await ValidateVideoAsync(
                        stream,
                        extension,
                        cancellationToken);

                    break;

                default:

                    throw new InvalidOperationException(
                        "نوع رسانه پشتیبانی نمی‌شود.");
            }
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static async Task ValidateImageAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = await ReadBytesAsync(
            stream,
            12,
            cancellationToken);

        var valid = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" =>
                IsJpeg(header),

            ".png" =>
                IsPng(header),

            ".gif" =>
                IsGif(header),

            ".webp" =>
                IsWebP(header),

            _ => false
        };

        if (!valid)
        {
            throw new InvalidOperationException(
                "ساختار واقعی فایل با پسوند آن مطابقت ندارد.");
        }
    }

    private static async Task ValidateAudioAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = await ReadBytesAsync(
            stream,
            12,
            cancellationToken);

        var valid = extension.ToLowerInvariant() switch
        {
            ".mp3" =>
                IsMp3(header),

            ".wav" =>
                IsWav(header),

            ".ogg" =>
                IsOgg(header),

            ".m4a" =>
                IsMp4Container(header),

            _ => false
        };

        if (!valid)
        {
            throw new InvalidOperationException(
                "ساختار واقعی فایل با پسوند آن مطابقت ندارد.");
        }
    }

    private static async Task ValidateVideoAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = await ReadBytesAsync(
            stream,
            12,
            cancellationToken);

        var valid = extension.ToLowerInvariant() switch
        {
            ".mp4" =>
                IsMp4Container(header),

            ".webm" =>
                IsWebM(header),

            ".mov" =>
                IsMov(header),

            _ => false
        };

        if (!valid)
        {
            throw new InvalidOperationException(
                "ساختار واقعی فایل با پسوند آن مطابقت ندارد.");
        }
    }

    private static async Task<byte[]> ReadBytesAsync(
        Stream stream,
        int count,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[count];

        var totalRead = 0;

        while (totalRead < count)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(totalRead, count - totalRead),
                cancellationToken);

            if (read == 0)
                break;

            totalRead += read;
        }

        if (totalRead < count)
            Array.Resize(ref buffer, totalRead);

        return buffer;
    }

    private static bool IsJpeg(byte[] data)
    {
        return data.Length >= 3 &&
               data[0] == 0xFF &&
               data[1] == 0xD8 &&
               data[2] == 0xFF;
    }

    private static bool IsPng(byte[] data)
    {
        return data.Length >= 8 &&
               data[0] == 0x89 &&
               data[1] == 0x50 &&
               data[2] == 0x4E &&
               data[3] == 0x47 &&
               data[4] == 0x0D &&
               data[5] == 0x0A &&
               data[6] == 0x1A &&
               data[7] == 0x0A;
    }

    private static bool IsGif(byte[] data)
    {
        return data.Length >= 6 &&
               data[0] == 'G' &&
               data[1] == 'I' &&
               data[2] == 'F' &&
               data[3] == '8' &&
               (data[4] == '7' || data[4] == '9') &&
               data[5] == 'a';
    }

    private static bool IsWebP(byte[] data)
    {
        return data.Length >= 12 &&
               data[0] == 'R' &&
               data[1] == 'I' &&
               data[2] == 'F' &&
               data[3] == 'F' &&
               data[8] == 'W' &&
               data[9] == 'E' &&
               data[10] == 'B' &&
               data[11] == 'P';
    }

    private static bool IsMp3(byte[] data)
    {
        if (data.Length >= 3 &&
            data[0] == 'I' &&
            data[1] == 'D' &&
            data[2] == '3')
        {
            return true;
        }

        return data.Length >= 2 &&
               data[0] == 0xFF &&
               (data[1] & 0xE0) == 0xE0;
    }

    private static bool IsWav(byte[] data)
    {
        return data.Length >= 12 &&
               data[0] == 'R' &&
               data[1] == 'I' &&
               data[2] == 'F' &&
               data[3] == 'F' &&
               data[8] == 'W' &&
               data[9] == 'A' &&
               data[10] == 'V' &&
               data[11] == 'E';
    }

    private static bool IsOgg(byte[] data)
    {
        return data.Length >= 4 &&
               data[0] == 'O' &&
               data[1] == 'g' &&
               data[2] == 'g' &&
               data[3] == 'S';
    }

    private static bool IsMp4Container(byte[] data)
    {
        return data.Length >= 8 &&
               data[4] == 'f' &&
               data[5] == 't' &&
               data[6] == 'y' &&
               data[7] == 'p';
    }

    private static bool IsWebM(byte[] data)
    {
        return data.Length >= 4 &&
               data[0] == 0x1A &&
               data[1] == 0x45 &&
               data[2] == 0xDF &&
               data[3] == 0xA3;
    }

    private static bool IsMov(byte[] data)
    {
        return IsMp4Container(data);
    }
}