using System.ComponentModel.DataAnnotations;

namespace MediaQR.Web.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}