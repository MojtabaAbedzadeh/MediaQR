using System.Net;
using System.Net.Http.Json;
using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaQR.Infrastructure.Services.Analytics.GeoIp;

public class IpApiGeoIpService : IGeoIpService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly GeoIpOptions _options;
    private readonly ILogger<IpApiGeoIpService> _logger;

    public IpApiGeoIpService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<GeoIpOptions> options,
        ILogger<IpApiGeoIpService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GeoIpResultDto?> ResolveAsync(
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            return null;
        }

        if (IsPrivateOrLoopback(ip))
        {
            return null;
        }

        var cacheKey = $"mediaqr:geoip:{ipAddress}";

        if (_cache.TryGetValue(
                cacheKey,
                out GeoIpResultDto? cachedResult))
        {
            return cachedResult;
        }

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                string.Empty);

            request.Content = JsonContent.Create(
                new
                {
                    q = ipAddress,
                    key = _options.ApiKey
                });

            using var response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "GeoIP API returned HTTP {StatusCode} for IP {IpAddress}.",
                    (int)response.StatusCode,
                    ipAddress);

                return null;
            }

            var apiResult =
                await response.Content
                    .ReadFromJsonAsync<IpApiResponse>(
                        cancellationToken);

            if (apiResult == null)
            {
                return null;
            }

            if (apiResult.IsBogon)
            {
                return null;
            }

            var result = new GeoIpResultDto
            {
                Country = Normalize(
                    apiResult.Location?.Country),

                City = Normalize(
                    apiResult.Location?.City)
            };

            var cacheOptions =
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(
                            Math.Max(
                                1,
                                _options.CacheMinutes))
                };

            _cache.Set(
                cacheKey,
                result,
                cacheOptions);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "GeoIP lookup failed for IP {IpAddress}.",
                ipAddress);

            return null;
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool IsPrivateOrLoopback(
        IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.AddressFamily ==
            System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            return
                bytes[0] == 10 ||
                (bytes[0] == 172 &&
                 bytes[1] >= 16 &&
                 bytes[1] <= 31) ||
                (bytes[0] == 192 &&
                 bytes[1] == 168);
        }

        if (ip.AddressFamily ==
            System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return
                ip.IsIPv6LinkLocal ||
                ip.IsIPv6SiteLocal ||
                ip.IsIPv6UniqueLocal;
        }

        return false;
    }
}