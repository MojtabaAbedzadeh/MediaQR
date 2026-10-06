using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MediaQR.Infrastructure.Services;

public class RequestContextService : IRequestContextService
{
    private const string VisitorCookieName = "mediaqr_visitor";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIpAddress()
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext == null)
            return null;

        return httpContext.Connection.RemoteIpAddress?
            .ToString();
    }

    public string? GetUserAgent()
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext == null)
            return null;

        if (!httpContext.Request.Headers.TryGetValue(
                "User-Agent",
                out var userAgent))
        {
            return null;
        }

        return userAgent.FirstOrDefault();
    }

    public string? GetReferrer()
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext == null)
            return null;

        if (!httpContext.Request.Headers.TryGetValue(
                "Referer",
                out var referer))
        {
            return null;
        }

        return referer.FirstOrDefault();
    }

    public string GetOrCreateVisitorId()
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return Guid.NewGuid().ToString("N");
        }

        if (httpContext.Request.Cookies.TryGetValue(
                VisitorCookieName,
                out var existingVisitorId) &&
            !string.IsNullOrWhiteSpace(existingVisitorId))
        {
            return existingVisitorId;
        }

        var visitorId =
            Guid.NewGuid().ToString("N");

        httpContext.Response.Cookies.Append(
            VisitorCookieName,
            visitorId,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(365)
            });

        return visitorId;
    }
}