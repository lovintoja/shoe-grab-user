using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ShoeGrabCommonModels;
using ShoeGrabUserManagement.Services;

namespace ShoeGrabTests;

public class JWTAuthenticationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig = new();
    private readonly JWTAuthenticationService _service;
    private readonly User _testUser;

    public JWTAuthenticationServiceTests()
    {
        SetupConfiguration();
        _service = new JWTAuthenticationService(_mockConfig.Object);

        _testUser = new User
        {
            Id = 123,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User,
            Profile = new UserProfile { PhoneNumber = "555-1234" }
        };
    }

    private void SetupConfiguration()
    {
        var configValues = new Dictionary<string, string>
        {
            ["JwtSettings:Secret"] = "super-secret-key-with-minimum-32-characters",
            ["JwtSettings:Issuer"] = "test-issuer",
            ["JwtSettings:Audience"] = "test-audience",
            ["JwtSettings:ExpiryHours"] = "1"
        };

        _mockConfig.Setup(x => x.GetSection("JwtSettings"))
            .Returns(new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build()
                .GetSection("JwtSettings"));
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsJWTToken()
    {
        // Act
        var token = _service.GenerateToken(_testUser);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaim()
    {
        // Act
        var token = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal(_testUser.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.Authentication).Value);
    }

    [Fact]
    public void GenerateToken_HasCorrectExpiration()
    {
        // Act
        var token = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddHours(1);
        Assert.True(jwt.ValidTo >= expectedExpiry.AddMinutes(-1) &&
                    jwt.ValidTo <= expectedExpiry.AddMinutes(1));
    }

    [Fact]
    public void GenerateToken_UsesCorrectSigningCredentials()
    {
        // Act
        var token = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                "super-secret-key-with-minimum-32-characters")),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        handler.ValidateToken(token, validationParameters, out _);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectIssuerAndAudience()
    {
        // Act
        var token = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
    }
}