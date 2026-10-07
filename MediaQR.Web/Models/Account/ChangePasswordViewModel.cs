using System.ComponentModel.DataAnnotations;

namespace MediaQR.Web.Models.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز عبور فعلی الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور فعلی")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور جدید")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "تکرار رمز عبور با رمز عبور جدید یکسان نیست.")]
    [Display(Name = "تکرار رمز عبور جدید")]
    public string ConfirmPassword { get; set; } = string.Empty;
}