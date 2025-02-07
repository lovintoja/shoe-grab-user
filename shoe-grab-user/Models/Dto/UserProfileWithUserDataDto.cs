namespace ShoeGrabUserManagement.Models.Dto;

public class UserProfileWithUserDataDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public UserProfileDto Profile { get; set; }
}
