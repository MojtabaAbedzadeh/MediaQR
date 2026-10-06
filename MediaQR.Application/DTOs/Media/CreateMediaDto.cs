using Microsoft.AspNetCore.Http;

namespace MediaQR.Application.DTOs.Media;

public class CreateMediaDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public IFormFile? File { get; set; }
}