using Microsoft.Extensions.Configuration;
using Npgsql;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence.Configurations.Common;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Authorisation checks for finance <see cref="SpaceModule"/> access using PostgreSQL (<c>NpgsqlCommand</c>)
/// coupled with cached <see cref="ISpaceMembershipEvaluator"/> lookups (avoiding interceptor cycles).
/// </summary>
public sealed class NpgsqlSpaceModuleAccessChecker : ISpaceModuleAccessChecker
{
    private readonly IConfiguration _configuration;
    private readonly ISpaceMembershipEvaluator _membership;

    /// <summary>Creates the checker.</summary>
    public NpgsqlSpaceModuleAccessChecker(
        IConfiguration configuration,
        ISpaceMembershipEvaluator membership)
    {
        _configuration = configuration;
        _membership = membership;
    }

    /// <inheritdoc/>
    public async Task<bool> HasSpaceModuleAccessAsync(
        Guid userId,
        Guid smoduleId,
        SpaceRole minimumRole = SpaceRole.Viewer,
        CancellationToken cancellationToken = default)
    {
        var moduleCodeFinance = SnakeCase.From(nameof(ModuleCode.Finance));

        var connectionString = _configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

        Guid? spaceId;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                SELECT space_id
                  FROM space_modules
                 WHERE id = @smodule_id
                   AND module_code = @module_code
                   AND is_enabled
                   AND NOT is_deleted
                """;

            var p = cmd.CreateParameter();
            p.ParameterName = "smodule_id";
            p.Value = smoduleId;
            cmd.Parameters.Add(p);

            var pCode = cmd.CreateParameter();
            pCode.ParameterName = "module_code";
            pCode.Value = moduleCodeFinance;
            cmd.Parameters.Add(pCode);

            var scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            spaceId = scalar switch
            {
                Guid g => g,
                _ => null,
            };
        }

        if (spaceId is null)
            return false;

        var role = await _membership
            .GetEffectiveRoleAsync(userId, spaceId.Value, cancellationToken)
            .ConfigureAwait(false);

        return role is not null && role.Value >= minimumRole;
    }
}
