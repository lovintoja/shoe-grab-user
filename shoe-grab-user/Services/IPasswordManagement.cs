namespace ShoeGrabUserManagement.Services;
public interface IPasswordManagement
{
    string HashPassword(string password);
    bool VerifyPassword(string inputPassword, string storedHash);
}
