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
}