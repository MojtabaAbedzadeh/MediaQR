using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MediaQR.Web.Models.Media;

public class CreateAudioMessageViewModel
{
    [Required(ErrorMessage = "عنوان پیام الزامی است.")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "عنوان پیام باید حداکثر 200 کاراکتر باشد.")]
    [Display(Name = "عنوان پیام")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد.")]
    [Display(Name = "متن پیام")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "انتخاب فایل صوتی الزامی است.")]
    [Display(Name = "فایل صوتی")]
    public IFormFile? AudioFile { get; set; }
}