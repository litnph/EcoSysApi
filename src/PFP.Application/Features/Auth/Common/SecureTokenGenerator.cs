using System.Security.Cryptography;

namespace PFP.Application.Features.Auth.Common;

/// <summary>Cryptographically strong opaque tokens for refresh, email verification, password reset, and GDPR flows.</summary>
public static class SecureTokenGenerator
{
    /// <summary>Creates a URL-safe token string derived from <paramref name="byteCount"/> random bytes.</summary>
    public static string CreateUrlSafe(int byteCount = 32)
    {
        var bytes = new byte[byteCount];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
