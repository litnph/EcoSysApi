using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the <c>dotnet ef</c> tooling (migrations, model validation).
/// <para>
/// Connection string resolution (first match wins):
/// <list type="number">
/// <item><c>PFP_DESIGN_CONNECTION</c> environment variable — explicit override for CI or ad-hoc
/// commands.</item>
/// <item><c>ConnectionStrings:Default</c> from the first <c>appsettings.Development.json</c> found
/// under a <c>PFP.API</c> folder by walking up from <see cref="Directory.GetCurrentDirectory"/> and
/// <see cref="AppContext.BaseDirectory"/> (so <c>dotnet ef</c> picks up the same Neon/local string as
/// the API when you run from the repo).</item>
/// <item>Fallback localhost string so the model still loads when no DB is configured.</item>
/// </list>
/// Runtime wiring remains in <see cref="InfrastructureServiceCollectionExtensions.AddInfrastructure"/>.
/// </para>
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc/>
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PFP_DESIGN_CONNECTION")
            ?? TryReadDefaultConnectionFromApiAppsettings()
            ?? "Host=localhost;Port=5432;Database=pfp_design;Username=pfp;Password=pfp";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .ReplaceService<IHistoryRepository, SnakeCaseNpgsqlHistoryRepository>()
            .Options;

        return new AppDbContext(options);
    }

    private static string? TryReadDefaultConnectionFromApiAppsettings()
    {
        foreach (var path in EnumeratePfpApiAppsettingsDevelopmentPaths())
        {
            if (!File.Exists(path)) continue;

            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var cs)) continue;
                if (!cs.TryGetProperty("Default", out var def)) continue;
                var s = def.GetString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            catch (JsonException)
            {
                // Malformed JSON — try next candidate path.
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumeratePfpApiAppsettingsDevelopmentPaths()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void TryAdd(string? p)
        {
            if (string.IsNullOrWhiteSpace(p)) return;
            try { roots.Add(Path.GetFullPath(p)); } catch { /* ignore invalid paths */ }
        }

        TryAdd(Directory.GetCurrentDirectory());
        TryAdd(AppContext.BaseDirectory);

        foreach (var root in roots)
        {
            var dir = new DirectoryInfo(root);
            for (var depth = 0; depth < 14 && dir != null; depth++, dir = dir.Parent!)
            {
                yield return Path.Combine(dir.FullName, "src", "PFP.API", "appsettings.Development.json");
                yield return Path.Combine(dir.FullName, "PFP.API", "appsettings.Development.json");
            }
        }
    }
}
