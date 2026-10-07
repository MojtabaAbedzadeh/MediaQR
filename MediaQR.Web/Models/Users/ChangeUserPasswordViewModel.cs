using System.ComponentModel.DataAnnotations;

namespace MediaQR.Web.Models.Users;

public class ChangeUserPasswordViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور جدید")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "تکرار رمز عبور با رمز عبور جدید یکسان نیست.")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}