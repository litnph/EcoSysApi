namespace PFP.Application.Common.Interfaces;

/// <summary>Hashes opaque bearer / refresh / reset tokens for storage (SHA-256, spec §6.1).</summary>
public interface ITokenHasher
{
    /// <summary>Returns a lowercase hex-encoded SHA-256 digest of <paramref name="plainToken"/>.</summary>
    string Sha256Hex(string plainToken);
}
