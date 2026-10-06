namespace MediaQR.Application.Options;

public class FileValidationOptions
{
    public long MaxImageSize { get; set; } = 20 * 1024 * 1024;

    public long MaxAudioSize { get; set; } = 100 * 1024 * 1024;

    public long MaxVideoSize { get; set; } = 1024 * 1024 * 1024;
}