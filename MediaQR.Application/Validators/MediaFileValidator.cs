using MediaQR.Application.Options;
using MediaQR.Domain.Entities;
using MediaQR.Domain.Enums;

namespace MediaQR.Application.Validators;

public class MediaFileValidator
{
    private static readonly Dictionary<string, MediaType> ExtensionMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            [".jpg"] = MediaType.Image,
            [".jpeg"] = MediaType.Image,
            [".png"] = MediaType.Image,
            [".webp"] = MediaType.Image,
            [".gif"] = MediaType.Image,

            // Audio
            [".mp3"] = MediaType.Audio,
            [".wav"] = MediaType.Audio,
            [".ogg"] = MediaType.Audio,
            [".m4a"] = MediaType.Audio,

            // Video
            [".mp4"] = MediaType.Video,
            [".webm"] = MediaType.Video,
            [".mov"] = MediaType.Video
        };

    private static readonly HashSet<string> AllowedImageMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };

    private static readonly HashSet<string> AllowedAudioMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "audio/mpeg",
            "audio/wav",
            "audio/wave",
            "audio/x-wav",
            "audio/ogg",
            "audio/mp4",
            "audio/x-m4a"
        };

    private static readonly HashSet<string> AllowedVideoMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

    public MediaType Validate(
        string fileName,
        string contentType,
        long fileSize,
        FileValidationOptions options)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException(
                "نام فایل مشخص نشده است.");

        if (fileSize <= 0)
            throw new InvalidOperationException(
                "فایل خالی است.");

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
            throw new InvalidOperationException(
                "پسوند فایل مشخص نشده است.");

        extension = extension.ToLowerInvariant();

        if (!ExtensionMap.TryGetValue(
                extension,
                out var mediaType))
        {
            throw new InvalidOperationException(
                "فرمت فایل مجاز نیست.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
            throw new InvalidOperationException(
                "نوع فایل مشخص نشده است.");

        var mimeTypeIsValid = mediaType switch
        {
            MediaType.Image =>
                AllowedImageMimeTypes.Contains(contentType),

            MediaType.Audio =>
                AllowedAudioMimeTypes.Contains(contentType),

            MediaType.Video =>
                AllowedVideoMimeTypes.Contains(contentType),

            _ => false
        };

        if (!mimeTypeIsValid)
        {
            throw new InvalidOperationException(
                "نوع MIME فایل با فرمت فایل مطابقت ندارد.");
        }

        var maxSize = mediaType switch
        {
            MediaType.Image => options.MaxImageSize,
            MediaType.Audio => options.MaxAudioSize,
            MediaType.Video => options.MaxVideoSize,
            _ => 0
        };

        if (fileSize > maxSize)
        {
            throw new InvalidOperationException(
                $"حجم فایل بیشتر از حد مجاز است. حداکثر حجم مجاز: {FormatSize(maxSize)}");
        }

        return mediaType;
    }

    private static string FormatSize(long bytes)
    {
        const long mb = 1024 * 1024;
        const long gb = 1024 * mb;

        if (bytes >= gb)
            return $"{bytes / (double)gb:0.##} GB";

        return $"{bytes / (double)mb:0.##} MB";
    }
}