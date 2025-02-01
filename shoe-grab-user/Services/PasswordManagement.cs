using System.Security.Cryptography;

namespace ShoeGrabUserManagement.Services;
public class PasswordManagement : IPasswordManagement
{
    private const int SaltSize = 16; // 128-bit salt
    private const int HashSize = 32; // 256-bit hash
    private const int Iterations = 10000; // PBKDF2 iterations

    public string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var saltAndHash = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, saltAndHash, 0, SaltSize);
        Array.Copy(hash, 0, saltAndHash, SaltSize, HashSize);

        return Convert.ToBase64String(saltAndHash);
    }

    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        var saltAndHashBytes = Convert.FromBase64String(storedHash);
        var salt = saltAndHashBytes[..SaltSize];
        var storedHashBytes = saltAndHashBytes[SaltSize..];

        using var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(HashSize);

        return computedHash.SequenceEqual(storedHashBytes);
    }
}
