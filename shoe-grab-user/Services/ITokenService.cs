using ShoeGrabCommonModels;

namespace ShoeGrabUserManagement.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
