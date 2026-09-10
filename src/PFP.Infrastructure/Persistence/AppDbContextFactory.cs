using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the <c>dotnet ef</c> tooling (migrations, model validation).
/// <para>
/// Connection string resolution (first match wins):
/// <list type="number">
/// <item><c>ConnectionStrings:Default</c> from <c>appsettings.json</c> and the active
/// environment-specific appsettings file under <c>PFP.API</c>.</item>
/// <item>The standard <c>ConnectionStrings__Default</c> environment variable override.</item>
/// </list>
/// Runtime wiring remains in <see cref="InfrastructureServiceCollectionExtensions.AddInfrastructure"/>.
/// </para>
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc/>
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiProjectDirectory = FindApiProjectDirectory()
            ?? throw new InvalidOperationException(
                "Could not locate the PFP.API project directory for design-time configuration.");
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is not configured for EF Core design-time tools.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }

    private static string? FindApiProjectDirectory()
    {
        foreach (var root in EnumerateSearchRoots())
        {
            var directory = new DirectoryInfo(root);
            for (var depth = 0; depth < 14 && directory is not null; depth++, directory = directory.Parent)
            {
                var candidates = new[]
                {
                    Path.Combine(directory.FullName, "src", "PFP.API"),
                    Path.Combine(directory.FullName, "PFP.API"),
                };

                foreach (var candidate in candidates)
                {
                    if (File.Exists(Path.Combine(candidate, "PFP.API.csproj"))
                        && File.Exists(Path.Combine(candidate, "appsettings.json")))
                    {
                        return candidate;
                    }
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void TryAdd(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                roots.Add(Path.GetFullPath(path));
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
            {
                // Ignore invalid host-provided paths and continue with the other search root.
            }
        }

        TryAdd(Directory.GetCurrentDirectory());
        TryAdd(AppContext.BaseDirectory);

        return roots;
    }
}
