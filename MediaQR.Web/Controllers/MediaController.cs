using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Authorize(Roles = "Admin")]
public class MediaController : Controller
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateMediaDto model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _mediaService.CreateAsync(
                model,
                cancellationToken);

            return View("Created", result);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                "File",
                ex.Message);

            return View(model);
        }
        catch (OperationCanceledException)
        {
            ModelState.AddModelError(
                "",
                "عملیات توسط کاربر لغو شد.");

            return View(model);
        }
        catch
        {
            ModelState.AddModelError(
                "",
                "خطایی هنگام آپلود فایل رخ داد. لطفاً دوباره تلاش کنید.");

            return View(model);
        }
    }
}