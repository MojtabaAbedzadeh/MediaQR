namespace MediaQR.Infrastructure.Options;

public class MediaUrlSecurityOptions
{
    public string SecretKey { get; set; } = string.Empty;

    public int LifetimeSeconds { get; set; } = 300;
}