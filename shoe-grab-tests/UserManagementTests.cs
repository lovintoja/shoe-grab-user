using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShoeGrabCommonModels;
using ShoeGrabCommonModels.Contexts;
using ShoeGrabUserManagement.Controllers;
using ShoeGrabUserManagement.Models.Dto;
using ShoeGrabUserManagement.Services;
using System.Security.Claims;

namespace ShoeGrabTests;

public class UserManagementControllerTests
{
    private readonly Mock<UserContext> _mockContext;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UserManagementController _controller;
    private readonly UserContextMockHelper _mockHelper;
    private readonly Mock<IPasswordManagement> _mockPasswordManagement;

    public UserManagementControllerTests()
    {
        _mockHelper = new UserContextMockHelper();
        _mockContext = _mockHelper.CreateMockContext();
        _mockTokenService = new Mock<ITokenService>();
        _mockPasswordManagement = new Mock<IPasswordManagement>();
        _controller = new UserManagementController(_mockContext.Object, _mockTokenService.Object, _mockPasswordManagement.Object);
    }

    private void SetAuthenticatedUser(int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Authentication, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var existingUser = new User { Email = "existing@example.com" };
        _mockHelper.Users.Add(existingUser);

        var request = new RegisterUserDto
        {
            Email = "existing@example.com",
            Username = "newuser",
            Password = "Password123!"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Email already in use.", conflictResult.Value?.GetType().GetProperty("Message")?.GetValue(conflictResult.Value));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "test"
        };
        _mockHelper.Users.Add(user);

        var request = new LoginUserDto
        {
            Email = "user@example.com",
            Password = "correctPassword"
        };

        _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("mock_token");
        _mockPasswordManagement.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("mock_token", okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task GetProfile_AuthenticatedUser_NoProfile_ReturnsNotFound()
    {
        // Arrange
        var testUserId = 1;
        var user = new User
        {
            Id = testUserId
        };
        _mockHelper.Users.Add(user);
        SetAuthenticatedUser(testUserId);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task EditProfile_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var testUserId = 1;
        var profile = new UserProfile
        {
            UserId = testUserId,
            Address = "Old Address"
        };
        _mockHelper.Profiles.Add(profile);
        SetAuthenticatedUser(testUserId);

        var updateDto = new EditProfileDto
        {
            Address = "New Address",
            PhoneNumber = "555-5678"
        };

        // Act
        var result = await _controller.EditProfile(updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("New Address", profile.Address);
        Assert.Equal("555-5678", profile.PhoneNumber);
    }

    [Fact]
    public async Task EditProfile_MissingProfile_ReturnsNotFound()
    {
        // Arrange
        SetAuthenticatedUser(999);
        var updateDto = new EditProfileDto();

        // Act
        var result = await _controller.EditProfile(updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}