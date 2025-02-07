using System.ComponentModel.DataAnnotations;

namespace ShoeGrabUserManagement.Models.Dto;
public class UserProfileDto
{
    [Required]
    public AddressDto Address { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    public string Bio { get; set; }
}
