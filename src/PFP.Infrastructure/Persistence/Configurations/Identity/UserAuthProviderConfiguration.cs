using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserAuthProvider"/>. Maps to <c>USER_AUTH_PROVIDERS</c>.</summary>
public sealed class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.Property(x => x.ProviderUserId).HasMaxLength(255);
        builder.Property(x => x.ProviderEmail).HasMaxLength(320);

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
               .WithMany(u => u.AuthProviders)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
