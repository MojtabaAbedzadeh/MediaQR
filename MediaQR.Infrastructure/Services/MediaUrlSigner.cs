using System.Security.Cryptography;
using System.Text;
using MediaQR.Application.Interfaces;
using MediaQR.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MediaQR.Infrastructure.Services;

public class MediaUrlSigner : IMediaUrlSigner
{
    private const string ContentPathTemplate =
        "/media/{0}/content";

    private readonly MediaUrlSecurityOptions _options;

    public MediaUrlSigner(
        IOptions<MediaUrlSecurityOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "MediaUrlSecurity:SecretKey is not configured.");
        }
    }

    public string CreateSignature(
        string code,
        DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Media code is required.",
                nameof(code));
        }

        var payload =
            BuildPayload(
                code,
                expiresAt);

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(
                    _options.SecretKey));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    public bool ValidateSignature(
        string code,
        DateTimeOffset expiresAt,
        string signature)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        var expectedSignature =
            CreateSignature(
                code,
                expiresAt);

        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string BuildPayload(
        string code,
        DateTimeOffset expiresAt)
    {
        var contentPath =
            string.Format(
                ContentPathTemplate,
                code);

        return
            $"{contentPath}|{expiresAt.ToUnixTimeSeconds()}";
    }
}