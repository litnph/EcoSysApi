using PFP.Application.Common.Constants;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>BCrypt password hashing with the platform-mandated work factor.</summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, AuthConstants.BcryptWorkFactor);

    /// <inheritdoc/>
    public bool Verify(string passwordHash, string plainPassword) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
}
