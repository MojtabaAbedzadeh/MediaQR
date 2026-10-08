using MediaQR.Application.DTOs.Media;
using MediaQR.Application.Interfaces;
using MediaQR.Web.Models.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Route("audio")]
[Authorize]
public class AudioMessageController : Controller
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<AudioMessageController> _logger;

    public AudioMessageController(
        IMediaService mediaService,
        ILogger<AudioMessageController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new CreateAudioMessageViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        CreateAudioMessageViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.AudioFile == null ||
            model.AudioFile.Length == 0)
        {
            ModelState.AddModelError(
                nameof(model.AudioFile),
                "لطفاً یک فایل صوتی انتخاب کنید.");

            return View(model);
        }

        try
        {
            var dto = new CreateMediaDto
            {
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                OrderNumber = string.IsNullOrWhiteSpace(model.OrderNumber) ? null : model.OrderNumber.Trim(),
                File = model.AudioFile
            };

            var result =
                await _mediaService.CreateAsync(
                    dto,
                    cancellationToken);

            return View("Created", result);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                nameof(model.AudioFile),
                ex.Message);

            return View(model);
        }
        catch (OperationCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "عملیات توسط کاربر لغو شد.");

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating audio message.");

            ModelState.AddModelError(
                string.Empty,
                "خطایی هنگام ایجاد پیام صوتی رخ داد. لطفاً دوباره تلاش کنید.");

            return View(model);
        }
    }
}