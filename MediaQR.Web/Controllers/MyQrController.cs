using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Authorize]
[Route("account/qr")]
public class MyQrController : Controller
{
    private readonly IQrManagementService _qrManagementService;
    private readonly ILogger<MyQrController> _logger;

    public MyQrController(
        IQrManagementService qrManagementService,
        ILogger<MyQrController> logger)
    {
        _qrManagementService = qrManagementService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var qrCodes =
                await _qrManagementService.GetAllAsync(
                    cancellationToken);

            return View(qrCodes);
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
                        "MyQr")
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
                "Error while loading QR codes for user {UserName}.",
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
            var qrCode =
                await _qrManagementService.GetByIdAsync(
                    id,
                    cancellationToken);

            if (qrCode == null)
            {
                return NotFound();
            }

            return View(qrCode);
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
                "Error while loading QR {QrCodeId} for user {UserName}.",
                id,
                User.Identity?.Name);

            return View("Error");
        }
    }

    [HttpPost("{id:long}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var qrCode =
                await _qrManagementService.GetByIdAsync(
                    id,
                    cancellationToken);

            if (qrCode == null)
            {
                return NotFound();
            }

            var newStatus = !qrCode.IsActive;

            var updated =
                await _qrManagementService.SetActiveAsync(
                    id,
                    newStatus,
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                newStatus
                    ? "QR Code فعال شد."
                    : "QR Code غیرفعال شد.";

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
                await _qrManagementService.DeleteAsync(
                    id,
                    cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "QR Code با موفقیت حذف شد.";

            return RedirectToAction(
                nameof(Index));
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
                nameof(Index));
        }
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(
    long id,
    CancellationToken cancellationToken)
    {
        try
        {
            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            var model = new MediaQR.Application.DTOs.Qr.UpdateQrCodeDto
            {
                Title = qrCode.Title,
                Description = qrCode.Description,
                ExpiresAt = qrCode.ExpiresAt,
                IsActive = qrCode.IsActive
            };

            ViewBag.QrCodeId = id;
            ViewBag.QrCode = qrCode.Code;

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading QR {QrCodeId} for editing.", id);
            return View("Error");
        }
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        MediaQR.Application.DTOs.Qr.UpdateQrCodeDto model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.QrCodeId = id;
                ViewBag.QrCode = await GetQrCodeForEditAsync(
                    id,
                    cancellationToken);

                return View(model);
            }

            var updated = await _qrManagementService.UpdateAsync(
                id,
                model,
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] = "اطلاعات QR با موفقیت ذخیره شد.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating QR {QrCodeId}.", id);
            ModelState.AddModelError(
                string.Empty,
                "ذخیره تغییرات انجام نشد. لطفاً دوباره تلاش کنید.");

            ViewBag.QrCodeId = id;
            return View(model);
        }
    }

    private async Task<string?> GetQrCodeForEditAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var qrCode = await _qrManagementService.GetByIdAsync(
            id,
            cancellationToken);

        return qrCode?.Code;
    }
}