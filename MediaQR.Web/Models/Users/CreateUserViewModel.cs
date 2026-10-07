using System.ComponentModel.DataAnnotations;

namespace MediaQR.Web.Models.Users;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست.")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "تکرار رمز عبور با رمز عبور یکسان نیست.")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نقش الزامی است.")]
    [Display(Name = "نقش")]
    public string Role { get; set; } = "Admin";
}