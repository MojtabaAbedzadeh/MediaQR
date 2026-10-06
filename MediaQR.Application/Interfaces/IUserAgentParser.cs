using MediaQR.Application.DTOs.Analytics;

namespace MediaQR.Application.Interfaces;

public interface IUserAgentParser
{
    UserAgentInfo Parse(string? userAgent);
}