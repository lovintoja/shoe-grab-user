using System.Security.Cryptography;

namespace ShoeGrabUserManagement.Services;
public class PasswordManagement : IPasswordManagement
{
    private const int SaltSize = 16; // 128-bit salt
    private const int HashSize = 32; // 256-bit hash
    private const int Iterations = 10000; // PBKDF2 iterations
    private byte[] Salt = [34, 122, 5, 143, 221, 79, 62, 39, 119, 87, 241, 10, 39, 248, 195, 102];

    public string HashPassword(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var saltAndHash = new byte[SaltSize + HashSize];
        Array.Copy(Salt, 0, saltAndHash, 0, SaltSize);
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
