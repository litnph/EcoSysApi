using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence.Configurations.Common;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Authorisation checks for finance <see cref="PFP.Domain.Entities.SpaceModule"/> access using ADO.NET against SQL Server,
/// coupled with cached <see cref="ISpaceMembershipEvaluator"/> lookups (avoiding interceptor cycles).
/// </summary>
public sealed class SqlConnectionSpaceModuleAccessChecker : ISpaceModuleAccessChecker
{
    private readonly IConfiguration _configuration;
    private readonly ISpaceMembershipEvaluator _membership;

    /// <summary>Creates the checker.</summary>
    public SqlConnectionSpaceModuleAccessChecker(
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
        await using (var conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                SELECT space_id
                  FROM space_modules
                 WHERE id = @smodule_id
                   AND module_code = @module_code
                   AND is_enabled = 1
                   AND is_deleted = 0
                """;

            var p = cmd.CreateParameter();
            p.ParameterName = "@smodule_id";
            p.Value = smoduleId;
            cmd.Parameters.Add(p);

            var pCode = cmd.CreateParameter();
            pCode.ParameterName = "@module_code";
            pCode.Value = moduleCodeFinance;
            cmd.Parameters.Add(pCode);

            var scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            spaceId = scalar switch
            {
                Guid g => g,
                byte[] bytes when bytes.Length == 16 => new Guid(bytes),
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
