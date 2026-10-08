using System.ComponentModel.DataAnnotations;

namespace MediaQR.Web.Models.Account;

public class RegisterViewModel
{
    [Required(
        ErrorMessage = "نام کاربری الزامی است.")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد.")]
    [RegularExpression(
        @"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد، نقطه، خط تیره و زیرخط باشد.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;


    [Required(
        ErrorMessage = "شماره همراه الزامی است.")]
    [RegularExpression(
        @"^09\d{9}$",
        ErrorMessage = "شماره همراه باید به صورت 11 رقمی و با 09 شروع شود.")]
    [Display(Name = "شماره همراه")]
    public string PhoneNumber { get; set; } = string.Empty;


    [Required(
        ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;


    [Required(
        ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [Compare(
        nameof(Password),
        ErrorMessage = "تکرار رمز عبور با رمز عبور مطابقت ندارد.")]
    [DataType(DataType.Password)]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}