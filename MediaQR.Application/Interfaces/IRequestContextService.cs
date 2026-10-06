namespace MediaQR.Application.Interfaces;

public interface IRequestContextService
{
    string? GetIpAddress();
    string? GetUserAgent();
    string? GetReferrer();
    string GetOrCreateVisitorId();
}