using System.ComponentModel.DataAnnotations;

namespace MediaQR.Application.DTOs.Media;

public class UpdateMediaDto
{
    [Required(
        ErrorMessage = "عنوان فایل الزامی است.")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "عنوان فایل باید بین 1 تا 200 کاراکتر باشد.")]
    [Display(Name = "عنوان")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد.")]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }
}