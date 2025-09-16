using System;

namespace Stott.Optimizely.RobotsHandler.Opal;

/// <summary>
/// Service for securely hashing and verifying bearer tokens
/// </summary>
public interface ITokenHashService
{
    /// <summary>
    /// Hashes a plain text token for secure storage using created date as salt
    /// </summary>
    /// <param name="token">The plain text token to hash</param>
    /// <param name="createdDate">The creation date to use as salt</param>
    /// <returns>Base64 encoded hash</returns>
    string HashToken(string token, DateTime createdDate);

    /// <summary>
    /// Verifies a plain text token against a stored hash using created date as salt
    /// </summary>
    /// <param name="token">The plain text token to verify</param>
    /// <param name="hash">The stored hash to verify against</param>
    /// <param name="createdDate">The creation date to use as salt</param>
    /// <returns>True if the token matches the hash</returns>
    bool VerifyToken(string token, string hash, DateTime createdDate);
}
