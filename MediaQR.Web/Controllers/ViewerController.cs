using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using MediaQR.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediaQR.Web.Controllers;

public class ViewerController : Controller
{
    private readonly IMediaViewerService _mediaViewerService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IMediaUrlSigner _mediaUrlSigner;
    private readonly MediaUrlSecurityOptions _mediaUrlSecurityOptions;
    private readonly ILogger<ViewerController> _logger;

    public ViewerController(
    IMediaViewerService mediaViewerService,
    IAnalyticsService analyticsService,
    IMediaUrlSigner mediaUrlSigner,
    IOptions<MediaUrlSecurityOptions> mediaUrlSecurityOptions,
    ILogger<ViewerController> logger)
    {
        _mediaViewerService = mediaViewerService;
        _analyticsService = analyticsService;
        _mediaUrlSigner = mediaUrlSigner;
        _mediaUrlSecurityOptions = mediaUrlSecurityOptions.Value;
        _logger = logger;
    }

    [HttpGet("/m/{code}")]
    public async Task<IActionResult> ViewMedia(
        string code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return NotFound();
        }

        var media = await _mediaViewerService.GetByCodeAsync(
            code,
            cancellationToken);

        if (media == null)
        {
            return NotFound();
        }

        try
        {
            await _analyticsService.TrackAsync(
                new TrackAnalyticsEventDto
                {
                    QrCodeId = media.QrCodeId,
                    EventType = AnalyticsEventType.Scan
                },
                cancellationToken);

            var expiresAt =
                DateTimeOffset.UtcNow.AddSeconds(
                    _mediaUrlSecurityOptions.LifetimeSeconds);

            var signature =
                _mediaUrlSigner.CreateSignature(
                    code,
                    expiresAt);

            var contentUrl = Url.Action(
                "Content",
                "Viewer",
                new
                {
                    code,
                    expires = expiresAt.ToUnixTimeSeconds(),
                    signature
                });

            media.ContentUrl = contentUrl;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام ثبت Scan Analytics. QrCodeId: {QrCodeId}",
                media.QrCodeId);
        }

        return View(media);
    }

    [HttpGet("/media/{code}/content")]
    public async Task<IActionResult> Content(
     string code,
     long expires,
     string signature,
     CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return NotFound();

        if (expires <= 0 || string.IsNullOrWhiteSpace(signature))
            return Unauthorized();

        DateTimeOffset expiresAt;

        try
        {
            expiresAt =
                DateTimeOffset.FromUnixTimeSeconds(expires);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Unauthorized();
        }

        if (!_mediaUrlSigner.ValidateSignature(
                code,
                expiresAt,
                signature))
        {
            return Unauthorized();
        }

        var media = await _mediaViewerService
            .GetByCodeAsync(
                code,
                cancellationToken);

        if (media == null)
            return NotFound();

        var stream = await _mediaViewerService
            .OpenContentAsync(
                code,
                cancellationToken);

        if (stream == null)
            return NotFound();

        return File(
            stream,
            media.ContentType,
            enableRangeProcessing: true);
    }
}