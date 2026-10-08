using MediaQR.Infrastructure.Identity;
using MediaQR.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "نام کاربری یا رمز عبور صحیح نیست.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "User {UserName} logged in successfully.",
                user.UserName);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning(
                "User {UserName} account is locked out.",
                user.UserName);

            ModelState.AddModelError(
                string.Empty,
                "حساب کاربری به دلیل تلاش‌های ناموفق متعدد موقتاً قفل شده است.");

            return View(model);
        }

        ModelState.AddModelError(
            string.Empty,
            "نام کاربری یا رمز عبور صحیح نیست.");

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        _logger.LogInformation(
            "User {UserName} logged out.",
            User.Identity?.Name);

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userName = model.UserName.Trim();
        var phoneNumber = NormalizePhoneNumber(model.PhoneNumber);

        // بررسی نام کاربری
        var existingUser =
            await _userManager.FindByNameAsync(userName);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                nameof(model.UserName),
                "این نام کاربری قبلاً استفاده شده است.");

            return View(model);
        }

        // بررسی شماره همراه
        var existingPhone =
            _userManager.Users
                .FirstOrDefault(x => x.PhoneNumber == phoneNumber);

        if (existingPhone != null)
        {
            ModelState.AddModelError(
                nameof(model.PhoneNumber),
                "این شماره همراه قبلاً ثبت شده است.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = false
        };

        var result =
            await _userManager.CreateAsync(
                user,
                model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    GetIdentityErrorMessage(error));
            }

            return View(model);
        }

        _logger.LogInformation(
            "New user {UserName} registered successfully.",
            user.UserName);

        TempData["Success"] =
            "ثبت‌نام با موفقیت انجام شد. اکنون می‌توانید وارد حساب کاربری خود شوید.";

        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("/account/access-denied")]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        var result =
            await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        _logger.LogInformation(
            "User {UserName} changed their password.",
            user.UserName);

        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] =
            "رمز عبور شما با موفقیت تغییر کرد.";

        return RedirectToAction(nameof(ChangePassword));
    }

    private static string GetIdentityErrorMessage(
    IdentityError error)
    {
        return error.Code switch
        {
            "PasswordTooShort" =>
                "رمز عبور باید حداقل 8 کاراکتر باشد.",

            "PasswordRequiresDigit" =>
                "رمز عبور باید حداقل شامل یک عدد باشد.",

            "PasswordRequiresLower" =>
                "رمز عبور باید حداقل شامل یک حرف کوچک انگلیسی باشد.",

            "PasswordRequiresUpper" =>
                "رمز عبور باید حداقل شامل یک حرف بزرگ انگلیسی باشد.",

            "DuplicateUserName" =>
                "این نام کاربری قبلاً استفاده شده است.",

            "DuplicateEmail" =>
                "این ایمیل قبلاً ثبت شده است.",

            _ =>
                "ثبت‌نام انجام نشد. لطفاً اطلاعات وارد شده را بررسی کنید."
        };
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        return phoneNumber
            .Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");
    }
}