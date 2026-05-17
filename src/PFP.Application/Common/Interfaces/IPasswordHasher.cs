namespace PFP.Application.Common.Interfaces;

/// <summary>Password hashing abstraction (bcrypt implementation lives in Infrastructure).</summary>
public interface IPasswordHasher
{
    /// <summary>Produces a bcrypt hash suitable for storing in <c>USERS.password_hash</c>.</summary>
    string Hash(string plainPassword);

    /// <summary>Returns <c>true</c> when <paramref name="plainPassword"/> matches <paramref name="passwordHash"/>.</summary>
    bool Verify(string passwordHash, string plainPassword);
}
