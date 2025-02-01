using ShoeGrabUserManagement.Services;

namespace ShoeGrabTests;

public class PasswordManagementTests
{
    private readonly PasswordManagement _passwordService = new();
    private const string TestPassword = "SecurePassword123!";
    private const string WrongPassword = "WrongPassword456!";

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyString()
    {
        // Act
        var hash = _passwordService.HashPassword(TestPassword);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_ValidPassword_Generates48ByteHash()
    {
        // Act
        var hash = _passwordService.HashPassword(TestPassword);
        var bytes = Convert.FromBase64String(hash);

        // Assert
        Assert.Equal(48, bytes.Length);
    }

    [Fact]
    public void HashPassword_SamePassword_DifferentHashes()
    {
        // Act
        var hash1 = _passwordService.HashPassword(TestPassword);
        var hash2 = _passwordService.HashPassword(TestPassword);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _passwordService.HashPassword(null));
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _passwordService.HashPassword(TestPassword);

        // Act
        var result = _passwordService.VerifyPassword(TestPassword, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _passwordService.HashPassword(TestPassword);

        // Act
        var result = _passwordService.VerifyPassword(WrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_TamperedHash_ReturnsFalse()
    {
        // Arrange
        var originalHash = _passwordService.HashPassword(TestPassword);
        var tamperedHash = TamperHash(originalHash);

        // Act
        var result = _passwordService.VerifyPassword(TestPassword, tamperedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_InvalidBase64_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            _passwordService.VerifyPassword(TestPassword, "invalid-base64"));
    }

    [Fact]
    public void VerifyPassword_NullInputPassword_ThrowsArgumentNullException()
    {
        var hash = _passwordService.HashPassword(TestPassword);
        Assert.Throws<ArgumentNullException>(() =>
            _passwordService.VerifyPassword(null, hash));
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_WorksCorrectly()
    {
        // Arrange
        var emptyPassword = string.Empty;
        var hash = _passwordService.HashPassword(emptyPassword);

        // Act
        var validResult = _passwordService.VerifyPassword(emptyPassword, hash);
        var invalidResult = _passwordService.VerifyPassword(" ", hash);

        // Assert
        Assert.True(validResult);
        Assert.False(invalidResult);
    }

    [Fact]
    public void Hash_And_Verify_WorkWithSpecialCharacters()
    {
        // Arrange
        var complexPassword = "P@$$w0rd_çà€Ω";
        var hash = _passwordService.HashPassword(complexPassword);

        // Act
        var result = _passwordService.VerifyPassword(complexPassword, hash);

        // Assert
        Assert.True(result);
    }

    private static string TamperHash(string originalHash)
    {
        var bytes = Convert.FromBase64String(originalHash);

        bytes[20] = (byte)(bytes[20] + 1);

        return Convert.ToBase64String(bytes);
    }
}