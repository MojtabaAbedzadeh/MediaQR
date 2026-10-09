using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Authorize]
[Route("account/media")]
public class MyMediaController : Controller
{
    private readonly IUserMediaService _userMediaService;
    private readonly ILogger<MyMediaController> _logger;
    private readonly IUserMediaAnalyticsService _userMediaAnalyticsService;

    public MyMediaController(
        IUserMediaService userMediaService,
        ILogger<MyMediaController> logger,
        IUserMediaAnalyticsService userMediaAnalyticsService)
    {
        _userMediaService = userMediaService;
        _logger = logger;
        _userMediaAnalyticsService = userMediaAnalyticsService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var mediaFiles =
                await _userMediaService.GetAllAsync(
                    cancellationToken);

            return View(mediaFiles);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        nameof(Index),
                        "MyMedia")
                });
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading media files for user {UserName}.",
                User.Identity?.Name);

            return View("Error");
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(
      long id,
      CancellationToken cancellationToken)
    {
        try
        {
            var media = await _userMediaService.GetByIdAsync(
                id,
                cancellationToken);

            if (media == null)
                return NotFound();

            var analytics =
                await _userMediaAnalyticsService.GetMediaAnalyticsAsync(
                    id,
                    cancellationToken);

            ViewBag.UserAnalytics = analytics;

            return View(media);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        nameof(Details),
                        "MyMedia",
                        new { id })
                });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading media {MediaId} and analytics for user {UserName}.",
                id,
                User.Identity?.Name);

            return View("Error");
        }
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(
    long id,
    CancellationToken cancellationToken)
    {
        try
        {
            var media = await _userMediaService.GetByIdAsync(
                id,
                cancellationToken);

            if (media == null)
                return NotFound();

            var model = new UpdateMediaDto
            {
                Title = media.Title,
                Description = media.Description,
                IsActive = media.IsActive
            };

            ViewBag.MediaId = id;
            ViewBag.Media = media;

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        nameof(Edit),
                        "MyMedia",
                        new { id })
                });
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading media {MediaId} for edit.",
                id);

            return View("Error");
        }
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    long id,
    UpdateMediaDto model,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var media = await _userMediaService.GetByIdAsync(
                id,
                cancellationToken);

            if (media == null)
                return NotFound();

            ViewBag.MediaId = id;
            ViewBag.Media = media;

            return View(model);
        }

        try
        {
            var updated = await _userMediaService.UpdateAsync(
                id,
                model,
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] =
                "اطلاعات فایل با موفقیت به‌روزرسانی شد.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(
                "Login",
                "Account");
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while updating media {MediaId}.",
                id);

            ModelState.AddModelError(
                string.Empty,
                "خطایی هنگام به‌روزرسانی فایل رخ داد.");

            var media = await _userMediaService.GetByIdAsync(
                id,
                cancellationToken);

            if (media == null)
                return NotFound();

            ViewBag.MediaId = id;
            ViewBag.Media = media;

            return View(model);
        }
    }

    [HttpPost("{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
    long id,
    CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _userMediaService.DeleteAsync(
                id,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "فایل با موفقیت حذف شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(
                "Login",
                "Account");
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while deleting media {MediaId}.",
                id);

            TempData["ErrorMessage"] =
                "خطایی هنگام حذف فایل رخ داد.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}