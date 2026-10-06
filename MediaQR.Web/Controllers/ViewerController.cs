using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

public class ViewerController : Controller
{
    private readonly IMediaViewerService _mediaViewerService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ViewerController> _logger;

    public ViewerController(
    IMediaViewerService mediaViewerService,
    IAnalyticsService analyticsService,
    ILogger<ViewerController> logger)
    {
        _mediaViewerService = mediaViewerService;
        _analyticsService = analyticsService;
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
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return NotFound();

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