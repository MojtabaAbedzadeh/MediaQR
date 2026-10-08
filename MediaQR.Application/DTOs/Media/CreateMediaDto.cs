using Microsoft.AspNetCore.Http;

namespace MediaQR.Application.DTOs.Media;

public class CreateMediaDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? PhoneNumber { get; set; }

    public string? OrderNumber { get; set; }

    public IFormFile? File { get; set; }
}