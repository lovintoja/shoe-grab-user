using System.ComponentModel.DataAnnotations;

namespace ShoeGrabUserManagement.Models.Dto;
public class EditProfileDto
{
    [Required]
    public string Address { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    public string Bio { get; set; }
}
