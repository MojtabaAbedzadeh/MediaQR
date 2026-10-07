using MediaQR.Application.Interfaces;
using MediaQR.Application.Options;
using MediaQR.Application.Validators;
using MediaQR.Infrastructure.Data;
using MediaQR.Infrastructure.Identity;
using MediaQR.Infrastructure.Options;
using MediaQR.Infrastructure.Qr;
using MediaQR.Infrastructure.Services;
using MediaQR.Infrastructure.Services.Analytics;
using MediaQR.Infrastructure.Services.Analytics.GeoIp;
using MediaQR.Infrastructure.Storage;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

builder.Services.Configure<FileValidationOptions>(
    builder.Configuration.GetSection("FileValidation"));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        1024L * 1024L * 1024L;
});

builder.Services.AddDbContext<MediaDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
    }
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        options.User.RequireUniqueEmail = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<MediaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";

    options.Cookie.Name = "MediaQR.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);

        return Task.CompletedTask;
    };
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        1024L * 1024L * 1024L;
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<MediaFileValidator>();
builder.Services.AddScoped<FileSignatureValidator>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IMediaViewerService, MediaViewerService>();
builder.Services.AddScoped<IQrManagementService, QrManagementService>();
builder.Services.AddScoped<IMediaManagementService, MediaManagementService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IRequestContextService, RequestContextService>();
builder.Services.AddScoped<IUserAgentParser, UserAgentParser>();
builder.Services.AddScoped<IAnalyticsReportService, AnalyticsReportService>();
builder.Services.Configure<GeoIpOptions>(builder.Configuration.GetSection("GeoIp"));
builder.Services.AddHttpClient<IGeoIpService, IpApiGeoIpService>(
    (serviceProvider, client) =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<GeoIpOptions>>()
                .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(
                Math.Max(1, options.TimeoutSeconds));
    });
builder.Services.Configure<MediaUrlSecurityOptions>(builder.Configuration.GetSection("MediaUrlSecurity"));
builder.Services.AddSingleton<IMediaUrlSigner, MediaUrlSigner>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var path =
                    httpContext.Request.Path;

                if (path.StartsWithSegments("/m") ||
                    path.StartsWithSegments("/media"))
                {
                    var ipAddress =
                        httpContext.Connection.RemoteIpAddress?
                            .ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        ipAddress,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                }

                return RateLimitPartition.GetNoLimiter(
                    "other");
            });
});

builder.Services.Configure<AnalyticsPrivacyOptions>(builder.Configuration.GetSection("AnalyticsPrivacy"));
builder.Services.AddHostedService<AnalyticsCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger =
        scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        builder.Configuration,
        logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] =
        "nosniff";

    context.Response.Headers["X-Frame-Options"] =
        "SAMEORIGIN";

    context.Response.Headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
