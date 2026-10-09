using System.ComponentModel.DataAnnotations;

namespace MediaQR.Application.DTOs.Qr;

public class UpdateQrCodeDto
{
    [StringLength(200, ErrorMessage = "عنوان حداکثر ۲۰۰ کاراکتر باشد.")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "توضیحات حداکثر ۲۰۰۰ کاراکتر باشد.")]
    public string? Description { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; }
}