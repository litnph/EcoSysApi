using System.Security.Cryptography;

namespace PFP.Infrastructure.Identity;

/// <summary>URL-safe opaque tokens for refresh sessions.</summary>
internal static class CryptoToken
{
    /// <summary>Creates a token from <paramref name="byteCount"/> random bytes (Base64url without padding).</summary>
    internal static string CreateUrlSafe(int byteCount = 32)
    {
        var bytes = new byte[byteCount];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
