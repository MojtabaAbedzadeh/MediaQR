using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Route("management/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsDashboardController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}