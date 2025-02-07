using System.ComponentModel.DataAnnotations;

namespace ShoeGrabUserManagement.Models.Dto;

public class EditPasswordDto
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }
}
