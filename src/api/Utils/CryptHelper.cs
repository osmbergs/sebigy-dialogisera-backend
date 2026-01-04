using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Sebigy.Dialogisera.Api.Utils;

public class CryptHelper
{
    private const int SaltSize = 128 / 8;  // 16 bytes
    private const int HashSize = 256 / 8;  // 32 bytes
    private const int Iterations = 100000;

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt + hash and encode as base64
        byte[] combined = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);

        return Convert.ToBase64String(combined);
    }

    public static bool IsHashEqual(string password, string hashedPassword)
    {
        byte[] combined = Convert.FromBase64String(hashedPassword);

        // Extract salt from stored hash
        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);

        // Extract original hash
        byte[] originalHash = new byte[HashSize];
        Buffer.BlockCopy(combined, SaltSize, originalHash, 0, HashSize);

        // Hash the input password with the same salt
        byte[] newHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Compare using constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
    }
}