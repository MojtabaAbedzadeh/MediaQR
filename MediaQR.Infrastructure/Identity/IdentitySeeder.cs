using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediaQR.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger logger)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // ---------------------------------------------------------
        // 1. Ensure Admin role exists
        // ---------------------------------------------------------

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(AdminRole));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    roleResult.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to create role '{AdminRole}': {errors}");
            }

            logger.LogInformation(
                "Identity role '{Role}' was created.",
                AdminRole);
        }

        // ---------------------------------------------------------
        // 2. Read Admin configuration
        // ---------------------------------------------------------

        var userName =
            configuration["MediaQR:Admin:UserName"];

        var email =
            configuration["MediaQR:Admin:Email"];

        var password =
            configuration["MediaQR:Admin:Password"];

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException(
                "MediaQR:Admin:UserName is not configured.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "MediaQR:Admin:Email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "MediaQR:Admin:Password is not configured.");
        }

        // ---------------------------------------------------------
        // 3. Find existing Admin user
        // ---------------------------------------------------------

        var user =
            await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    createResult.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to create Admin user '{userName}': {errors}");
            }

            logger.LogInformation(
                "Admin user '{UserName}' was created.",
                userName);
        }

        // ---------------------------------------------------------
        // 4. Ensure Admin role assignment
        // ---------------------------------------------------------

        if (!await userManager.IsInRoleAsync(user, AdminRole))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    AdminRole);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    roleResult.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to assign role '{AdminRole}' to user '{userName}': {errors}");
            }

            logger.LogInformation(
                "Admin role assigned to user '{UserName}'.",
                userName);
        }
    }
}