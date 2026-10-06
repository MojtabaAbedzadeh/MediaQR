using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Route("management/media")]
public class MediaManagementController : Controller
{
    private readonly IMediaManagementService _mediaManagementService;
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaManagementController> _logger;

    public MediaManagementController(
        IMediaManagementService mediaManagementService,
        IMediaService mediaService,
        ILogger<MediaManagementController> logger)
    {
        _mediaManagementService = mediaManagementService;
        _mediaService = mediaService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var mediaFiles =
            await _mediaManagementService.GetAllAsync(
                cancellationToken);

        return View(mediaFiles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        var media =
            await _mediaManagementService.GetByIdAsync(
                id,
                cancellationToken);

        if (media == null)
            return NotFound();

        return View(media);
    }

    [HttpPost("{id:long}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(
        long id,
        CancellationToken cancellationToken)
    {
        var media =
            await _mediaManagementService.GetByIdAsync(
                id,
                cancellationToken);

        if (media == null)
            return NotFound();

        var newStatus = !media.IsActive;

        var updated =
            await _mediaManagementService.SetActiveAsync(
                id,
                newStatus,
                cancellationToken);

        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] =
            newStatus
                ? "رسانه فعال شد."
                : "رسانه غیرفعال شد.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    [HttpPost("{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted =
                await _mediaManagementService.DeleteAsync(
                    id,
                    cancellationToken);

            if (!deleted)
                return NotFound();

            TempData["SuccessMessage"] =
                "رسانه با موفقیت حذف شد.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new CreateMediaDto());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateMediaDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var result =
                await _mediaService.CreateAsync(
                    dto,
                    cancellationToken);

            TempData["SuccessMessage"] =
                "رسانه با موفقیت ایجاد شد.";

            return View(
                "CreateResult",
                result);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا هنگام ایجاد رسانه. Title: {Title}",
                dto.Title);

            ModelState.AddModelError(
                string.Empty,
                "در هنگام ثبت رسانه خطایی رخ داد. لطفاً دوباره تلاش کنید.");

            return View(dto);
        }
    }
}