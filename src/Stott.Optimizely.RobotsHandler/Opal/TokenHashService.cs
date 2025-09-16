using System;
using System.Security.Cryptography;
using System.Text;

namespace Stott.Optimizely.RobotsHandler.Opal;

/// <summary>
/// Secure token hashing service implementation using PBKDF2 with SHA256 and created date as salt
/// </summary>
internal sealed class TokenHashService : ITokenHashService
{
    private const int HashSize = 32;
    private const int Iterations = 100000; // PBKDF2 iterations for security

    public string HashToken(string token, DateTime createdDate)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty");
        }

        // Hash the token with the date-based salt using PBKDF2
        var salt = GenerateDateBasedSalt(createdDate);
        using (var pbkdf2 = new Rfc2898DeriveBytes(token, salt, Iterations, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(HashSize);
            return Convert.ToBase64String(hash);
        }
    }

    public bool VerifyToken(string token, string hash, DateTime createdDate)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var expectedHash = HashToken(token, createdDate);
            
            // Constant-time comparison to prevent timing attacks
            if (expectedHash.Length != hash.Length)
                return false;

            int result = 0;
            for (int i = 0; i < expectedHash.Length; i++)
            {
                result |= expectedHash[i] ^ hash[i];
            }
            
            return result == 0;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] GenerateDateBasedSalt(DateTime createdDate)
    {
        // Convert the created date to a consistent string format (UTC to avoid timezone issues)
        var dateString = createdDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        return Encoding.UTF8.GetBytes(dateString);
    }
}