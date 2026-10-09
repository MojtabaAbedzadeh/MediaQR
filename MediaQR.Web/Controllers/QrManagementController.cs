using MediaQR.Application.DTOs.Qr;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Route("management/qr")]
[Authorize(Roles = "Admin")]
public class QrManagementController : Controller
{
    private readonly IQrManagementService _qrManagementService;
    private readonly ILogger<QrManagementController> _logger;

    public QrManagementController(
        IQrManagementService qrManagementService,
        ILogger<QrManagementController> logger)
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
            var qrCodes = await _qrManagementService.GetAllAsync(
                cancellationToken);

            return View(qrCodes);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading QR codes.");

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
            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            return View(qrCode);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading QR code {QrCodeId}.", id);

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
            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            var model = new UpdateQrCodeDto
            {
                Title = qrCode.Title,
                Description = qrCode.Description,
                ExpiresAt = qrCode.ExpiresAt,
                IsActive = qrCode.IsActive
            };

            ViewBag.QrCodeId = id;
            ViewBag.QrCode = qrCode;

            return View(model);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error loading QR code {QrCodeId} for edit.", id);

            return View("Error");
        }
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        UpdateQrCodeDto model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.QrCodeId = id;

            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            ViewBag.QrCode = qrCode;

            return View(model);
        }

        try
        {
            var updated = await _qrManagementService.UpdateAsync(
                id,
                model,
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] = "اطلاعات QR Code با موفقیت به‌روزرسانی شد.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error updating QR code {QrCodeId}.", id);

            ModelState.AddModelError(
                string.Empty,
                "خطا در به‌روزرسانی QR Code.");

            ViewBag.QrCodeId = id;

            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            ViewBag.QrCode = qrCode;

            return View(model);
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
            var qrCode = await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

            if (qrCode == null)
                return NotFound();

            var newStatus = !qrCode.IsActive;

            var updated = await _qrManagementService.SetActiveAsync(
                id,
                newStatus,
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] = newStatus
                ? "QR Code فعال شد."
                : "QR Code غیرفعال شد.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error changing QR status for {QrCodeId}.", id);

            TempData["ErrorMessage"] = "خطا در تغییر وضعیت QR Code.";

            return RedirectToAction(nameof(Index));
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
            var deleted = await _qrManagementService.DeleteAsync(
                id,
                cancellationToken);

            if (!deleted)
                return NotFound();

            TempData["SuccessMessage"] = "QR Code با موفقیت حذف شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error deleting QR code {QrCodeId}.", id);

            TempData["ErrorMessage"] = "خطا در حذف QR Code.";

            return RedirectToAction(nameof(Index));
        }
    }
}