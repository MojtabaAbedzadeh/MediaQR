using MediaQR.Infrastructure.Identity;
using MediaQR.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediaQR.Web.Controllers;

[Authorize(Roles = IdentitySeeder.AdminRole)]
[Route("management/users")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users
            .OrderBy(x => x.UserName)
            .ToList();

        var model = new UserListViewModel();

        foreach (var user in users)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            model.Users.Add(new UserItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut =
                    await _userManager.IsLockedOutAsync(user),
                Roles = roles.ToArray()
            });
        }

        return View(model);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new CreateUserViewModel
        {
            Role = IdentitySeeder.AdminRole
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "نقش انتخاب‌شده معتبر نیست.");

            return View(model);
        }

        var existingUser =
            await _userManager.FindByNameAsync(model.UserName);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                nameof(model.UserName),
                "این نام کاربری قبلاً استفاده شده است.");

            return View(model);
        }

        var existingEmail =
            await _userManager.FindByEmailAsync(model.Email);

        if (existingEmail != null)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "این ایمیل قبلاً استفاده شده است.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true
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
                    error.Description);
            }

            return View(model);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                model.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        _logger.LogInformation(
            "Admin {AdminUser} created user {CreatedUser}.",
            User.Identity?.Name,
            user.UserName);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("lock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (IsCurrentUser(user))
        {
            TempData["Error"] =
                "نمی‌توانید حساب کاربری خودتان را قفل کنید.";

            return RedirectToAction(nameof(Index));
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        if (roles.Contains(IdentitySeeder.AdminRole))
        {
            var adminCount =
                await CountAdminsAsync();

            if (adminCount <= 1)
            {
                TempData["Error"] =
                    "آخرین کاربر Admin قابل قفل شدن نیست.";

                return RedirectToAction(nameof(Index));
            }
        }

        await _userManager.SetLockoutEnabledAsync(
            user,
            true);

        await _userManager.SetLockoutEndDateAsync(
            user,
            DateTimeOffset.UtcNow.AddYears(100));

        _logger.LogWarning(
            "Admin {AdminUser} locked user {TargetUser}.",
            User.Identity?.Name,
            user.UserName);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("unlock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(
            user,
            null);

        _logger.LogInformation(
            "Admin {AdminUser} unlocked user {TargetUser}.",
            User.Identity?.Name,
            user.UserName);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (IsCurrentUser(user))
        {
            TempData["Error"] =
                "نمی‌توانید حساب کاربری خودتان را حذف کنید.";

            return RedirectToAction(nameof(Index));
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        if (roles.Contains(IdentitySeeder.AdminRole))
        {
            var adminCount =
                await CountAdminsAsync();

            if (adminCount <= 1)
            {
                TempData["Error"] =
                    "آخرین کاربر Admin قابل حذف نیست.";

                return RedirectToAction(nameof(Index));
            }
        }

        var result =
            await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning(
            "Admin {AdminUser} deleted user {DeletedUser}.",
            User.Identity?.Name,
            user.UserName);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (IsCurrentUser(user))
        {
            TempData["Error"] =
                "برای تغییر رمز عبور حساب خودتان از بخش تغییر رمز شخصی استفاده کنید.";

            return RedirectToAction(nameof(Index));
        }

        return View(new ChangeUserPasswordViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty
        });
    }

    [HttpPost("change-password/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        string id,
        ChangeUserPasswordViewModel model)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (IsCurrentUser(user))
        {
            TempData["Error"] =
                "برای تغییر رمز عبور حساب خودتان از بخش تغییر رمز شخصی استفاده کنید.";

            return RedirectToAction(nameof(Index));
        }

        model.UserId = user.Id;
        model.UserName = user.UserName ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                token,
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

        // تغییر SecurityStamp باعث بی‌اعتبار شدن
        // Sessionهای قبلی کاربر می‌شود.
        await _userManager.UpdateSecurityStampAsync(user);

        _logger.LogWarning(
            "Admin {AdminUser} changed password for user {TargetUser}.",
            User.Identity?.Name,
            user.UserName);

        TempData["Success"] =
            $"رمز عبور کاربر «{user.UserName}» با موفقیت تغییر کرد.";

        return RedirectToAction(nameof(Index));
    }

    private bool IsCurrentUser(ApplicationUser user)
    {
        return string.Equals(
            _userManager.GetUserId(User),
            user.Id,
            StringComparison.Ordinal);
    }

    private async Task<int> CountAdminsAsync()
    {
        var admins =
            await _userManager.GetUsersInRoleAsync(
                IdentitySeeder.AdminRole);

        return admins.Count;
    }
}