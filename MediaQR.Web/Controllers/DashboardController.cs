using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Authorize]
[Route("account/dashboard")]
public class DashboardController : Controller
{
    private readonly IUserDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IUserDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var dashboard =
                await _dashboardService.GetDashboardAsync(
                    cancellationToken);

            return View(dashboard);
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
                        "Dashboard")
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
                "Error while loading dashboard for user {UserName}.",
                User.Identity?.Name);

            return View("Error");
        }
    }
}