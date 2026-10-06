using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using MediaQR.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track(
        [FromBody] TrackAnalyticsEventDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsService.TrackAsync(
                dto,
                cancellationToken);

            return Ok(new
            {
                success = true
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "درخواست نامعتبر برای ثبت Analytics دریافت شد.");

            return BadRequest(new
            {
                success = false,
                message = "اطلاعات رویداد نامعتبر است."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام ثبت Analytics Event.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام ثبت رویداد."
                });
        }
    }
}