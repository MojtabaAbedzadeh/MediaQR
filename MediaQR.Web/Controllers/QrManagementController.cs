using MediaQR.Application.DTOs.Qr;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Route("management/qr")]
public class QrManagementController : Controller
{
    private readonly IQrManagementService _qrManagementService;

    public QrManagementController(
        IQrManagementService qrManagementService)
    {
        _qrManagementService = qrManagementService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var qrCodes =
            await _qrManagementService.GetAllAsync(
                cancellationToken);

        return View(qrCodes);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        var qrCode =
            await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

        if (qrCode == null)
            return NotFound();

        return View(qrCode);
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(
        long id,
        CancellationToken cancellationToken)
    {
        var qrCode =
            await _qrManagementService.GetByIdAsync(
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

        ViewBag.QrCodeId = qrCode.Id;
        ViewBag.QrCode = qrCode.Code;
        ViewBag.MediaTitle = qrCode.MediaTitle;

        return View(model);
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        UpdateQrCodeDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.QrCodeId = id;

            return View(dto);
        }

        var updated =
            await _qrManagementService.UpdateAsync(
                id,
                dto,
                cancellationToken);

        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] =
            "اطلاعات QR با موفقیت بروزرسانی شد.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    [HttpPost("{id:long}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(
        long id,
        CancellationToken cancellationToken)
    {
        var qrCode =
            await _qrManagementService.GetByIdAsync(
                id,
                cancellationToken);

        if (qrCode == null)
            return NotFound();

        var newStatus = !qrCode.IsActive;

        var updated =
            await _qrManagementService.SetActiveAsync(
                id,
                newStatus,
                cancellationToken);

        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] =
            newStatus
                ? "QR فعال شد."
                : "QR غیرفعال شد.";

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
        var deleted =
            await _qrManagementService.DeleteAsync(
                id,
                cancellationToken);

        if (!deleted)
            return NotFound();

        TempData["SuccessMessage"] =
            "QR با موفقیت حذف شد.";

        return RedirectToAction(
            nameof(Index));
    }
}