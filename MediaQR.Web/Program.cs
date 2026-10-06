using MediaQR.Application.Interfaces;
using MediaQR.Application.Options;
using MediaQR.Application.Validators;
using MediaQR.Infrastructure.Data;
using MediaQR.Infrastructure.Qr;
using MediaQR.Infrastructure.Services;
using MediaQR.Infrastructure.Storage;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        1024L * 1024L * 1024L;
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
