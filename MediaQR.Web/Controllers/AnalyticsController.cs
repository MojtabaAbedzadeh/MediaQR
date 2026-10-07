using MediaQR.Application.DTOs.Analytics;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IAnalyticsReportService _analyticsReportService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        IAnalyticsReportService analyticsReportService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _analyticsReportService = analyticsReportService;
        _logger = logger;
    }

    [HttpPost("track")]
    [AllowAnonymous]
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

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        try
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value >= to.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بازه زمانی نامعتبر است."
                });
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                From = from,
                To = to
            };

            var result = await _analyticsReportService
                .GetSummaryAsync(
                    qrCodeId,
                    dateRange,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
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
                "درخواست نامعتبر برای دریافت Analytics Summary.");

            return BadRequest(new
            {
                success = false,
                message = "پارامترهای گزارش نامعتبر هستند."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام دریافت Analytics Summary.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام دریافت گزارش Analytics."
                });
        }
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        try
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value >= to.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بازه زمانی نامعتبر است."
                });
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                From = from,
                To = to
            };

            var result = await _analyticsReportService
                .GetTimelineAsync(
                    qrCodeId,
                    dateRange,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام دریافت Analytics Timeline.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام دریافت Timeline Analytics."
                });
        }
    }

    [HttpGet("breakdown/device")]
    public async Task<IActionResult> GetDeviceBreakdown(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        if (from.HasValue &&
            to.HasValue &&
            from.Value >= to.Value)
        {
            return BadRequest(new
            {
                success = false,
                message = "بازه زمانی نامعتبر است."
            });
        }

        var result = await _analyticsReportService
            .GetDeviceBreakdownAsync(
                qrCodeId,
                new AnalyticsDateRangeDto
                {
                    From = from,
                    To = to
                },
                cancellationToken);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet("breakdown/os")]
    public async Task<IActionResult> GetOperatingSystemBreakdown(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        if (from.HasValue &&
            to.HasValue &&
            from.Value >= to.Value)
        {
            return BadRequest(new
            {
                success = false,
                message = "بازه زمانی نامعتبر است."
            });
        }

        var result = await _analyticsReportService
            .GetOperatingSystemBreakdownAsync(
                qrCodeId,
                new AnalyticsDateRangeDto
                {
                    From = from,
                    To = to
                },
                cancellationToken);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet("breakdown/browser")]
    public async Task<IActionResult> GetBrowserBreakdown(
        [FromQuery] long? qrCodeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (from.HasValue &&
            to.HasValue &&
            from.Value >= to.Value)
        {
            return BadRequest(new
            {
                success = false,
                message = "بازه زمانی نامعتبر است."
            });
        }

        var result = await _analyticsReportService
            .GetBrowserBreakdownAsync(
                qrCodeId,
                new AnalyticsDateRangeDto
                {
                    From = from,
                    To = to
                },
                cancellationToken);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet("breakdown/referrer")]
    public async Task<IActionResult> GetReferrerBreakdown(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        try
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value >= to.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بازه زمانی نامعتبر است."
                });
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                From = from,
                To = to
            };

            var result = await _analyticsReportService
                .GetReferrerBreakdownAsync(
                    qrCodeId,
                    dateRange,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
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
                "درخواست نامعتبر برای دریافت Referrer Breakdown.");

            return BadRequest(new
            {
                success = false,
                message = "پارامترهای گزارش نامعتبر هستند."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام دریافت Referrer Breakdown.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام دریافت گزارش Referrer."
                });
        }
    }

    [HttpGet("breakdown/country")]
    public async Task<IActionResult> GetCountryBreakdown(
     [FromQuery] long? qrCodeId,
     [FromQuery] DateTime? from,
     [FromQuery] DateTime? to,
     CancellationToken cancellationToken)
    {
        try
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value >= to.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بازه زمانی نامعتبر است."
                });
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                From = from,
                To = to
            };

            var result = await _analyticsReportService
                .GetCountryBreakdownAsync(
                    qrCodeId,
                    dateRange,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
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
                "درخواست نامعتبر برای دریافت Country Breakdown.");

            return BadRequest(new
            {
                success = false,
                message = "پارامترهای گزارش نامعتبر هستند."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام دریافت Country Breakdown.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام دریافت گزارش کشور."
                });
        }
    }

    [HttpGet("breakdown/city")]
    public async Task<IActionResult> GetCityBreakdown(
    [FromQuery] long? qrCodeId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        try
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value >= to.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بازه زمانی نامعتبر است."
                });
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                From = from,
                To = to
            };

            var result = await _analyticsReportService
                .GetCityBreakdownAsync(
                    qrCodeId,
                    dateRange,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
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
                "درخواست نامعتبر برای دریافت City Breakdown.");

            return BadRequest(new
            {
                success = false,
                message = "پارامترهای گزارش نامعتبر هستند."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام دریافت City Breakdown.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطا هنگام دریافت گزارش شهر."
                });
        }
    }

    [HttpGet("qr-codes")]
    public async Task<IActionResult> SearchQrCodes(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var result =
                await _analyticsReportService.SearchQrCodesAsync(
                    search,
                    page,
                    pageSize,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام جستجوی QR Code های Analytics.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "خطایی هنگام دریافت QR Code ها رخ داد."
                });
        }
    }
}