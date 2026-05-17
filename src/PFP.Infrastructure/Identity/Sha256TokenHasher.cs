using System.Security.Cryptography;
using System.Text;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>SHA-256 helper for refresh, reset, and verification tokens.</summary>
public sealed class Sha256TokenHasher : ITokenHasher
{
    /// <inheritdoc/>
    public string Sha256Hex(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
