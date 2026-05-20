using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace PFP.IntegrationTests.Support;

/// <summary>
/// Hosts <see cref="Program"/> against SQL Server. Either:
/// <list type="bullet">
/// <item>Set <c>PFP_INTEGRATION_CONNECTION</c> to a full SQL Server connection string, or</item>
/// <item>Set <c>PFP_INTEGRATION_CONNECTION_FILE</c> to a file path whose contents are that connection string, or</item>
/// <item>Run Docker locally so Testcontainers can start <c>mcr.microsoft.com/mssql/server:2022-latest</c>.</item>
/// </list>
/// EF migrations run from the test class <c>IAsyncLifetime.InitializeAsync</c> hook.
/// </summary>
public sealed class IntegrationTestFixture : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer? _msSql;
    private readonly string _connectionString;

    /// <summary>Creates the factory and prepares the database connection.</summary>
    public IntegrationTestFixture()
    {
        var configured = TryGetExternalConnectionString();
        if (configured is not null)
        {
            _connectionString = configured;
            return;
        }

        try
        {
            _msSql = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("PfpTest_Integration_2026_Aa1")
                .Build();

            _msSql.StartAsync().GetAwaiter().GetResult();
            _connectionString = _msSql.GetConnectionString();
        }
        catch (ArgumentException ex) when (ex.ParamName is "DockerEndpointAuthConfig")
        {
            throw new InvalidOperationException(
                "Integration tests need SQL Server. Start Docker Desktop so Testcontainers can run mssql/server:2022-latest, " +
                "or set PFP_INTEGRATION_CONNECTION (or PFP_INTEGRATION_CONNECTION_FILE) to a SQL Server connection string. " +
                "See IntegrationTestFixture remarks.",
                ex);
        }
    }

    private static string? TryGetExternalConnectionString()
    {
        var envCs = Environment.GetEnvironmentVariable("PFP_INTEGRATION_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envCs))
            return envCs.Trim();

        var filePath = Environment.GetEnvironmentVariable("PFP_INTEGRATION_CONNECTION_FILE");
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        filePath = filePath.Trim();
        if (!File.Exists(filePath))
            throw new InvalidOperationException(
                $"PFP_INTEGRATION_CONNECTION_FILE is set to '{filePath}' but the file does not exist.");

        return File.ReadAllText(filePath).Trim();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && _msSql is not null)
            _msSql.DisposeAsync().AsTask().GetAwaiter().GetResult();

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("Jwt:Secret", new string('x', 48));
        builder.UseSetting("Hangfire:DisableServer", "true");
        builder.UseSetting("Hangfire:DisableRecurringRegistration", "true");
    }
}
