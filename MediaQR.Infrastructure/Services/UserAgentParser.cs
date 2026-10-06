using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using UAParser;

namespace MediaQR.Infrastructure.Services;

public class UserAgentParser : IUserAgentParser
{
    private readonly Parser _parser;

    public UserAgentParser()
    {
        _parser = Parser.GetDefault();
    }

    public UserAgentInfo Parse(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new UserAgentInfo();
        }

        var clientInfo = _parser.Parse(userAgent);

        return new UserAgentInfo
        {
            DeviceType = GetDeviceType(clientInfo),
            OperatingSystem = BuildOperatingSystem(clientInfo),
            Browser = clientInfo.UA.Family,
            BrowserVersion = BuildBrowserVersion(clientInfo)
        };
    }

    private static string GetDeviceType(ClientInfo clientInfo)
    {
        var family = clientInfo.Device.Family;

        if (string.IsNullOrWhiteSpace(family) ||
            family.Equals("Other", StringComparison.OrdinalIgnoreCase))
        {
            return "Desktop";
        }

        if (family.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
            family.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
        {
            return "Tablet";
        }

        if (family.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
            family.Contains("Phone", StringComparison.OrdinalIgnoreCase) ||
            family.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
            family.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }

        return "Desktop";
    }

    private static string? BuildOperatingSystem(ClientInfo clientInfo)
    {
        var os = clientInfo.OS;

        if (os == null ||
            string.IsNullOrWhiteSpace(os.Family))
        {
            return null;
        }

        var version = BuildVersion(
            os.Major,
            os.Minor,
            os.Patch);

        return string.IsNullOrWhiteSpace(version)
            ? os.Family
            : $"{os.Family} {version}";
    }

    private static string? BuildBrowserVersion(ClientInfo clientInfo)
    {
        var ua = clientInfo.UA;

        if (ua == null ||
            string.IsNullOrWhiteSpace(ua.Family))
        {
            return null;
        }

        var version = BuildVersion(
            ua.Major,
            ua.Minor,
            ua.Patch);

        return version;
    }

    private static string? BuildVersion(
    string? major,
    string? minor,
    string? patch)
    {
        var parts = new[]
        {
        major,
        minor,
        patch
    }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .ToArray();

        return parts.Length == 0
            ? null
            : string.Join(".", parts);
    }
}