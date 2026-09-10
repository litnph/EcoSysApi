using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PFP.Infrastructure;
using PFP.Infrastructure.Persistence;
using Xunit;

namespace PFP.IntegrationTests.Configuration;

public sealed class DatabaseConfigurationTests
{
    [Fact]
    public void AddInfrastructure_UsesDefaultConnectionStringWithSqlServer()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Server=.\\SQLEXPRESS;Database=configuration_test;Trusted_Connection=True;TrustServerCertificate=True",
            })
            .Build();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", dbContext.Database.ProviderName);
    }

    [Fact]
    public void AddInfrastructure_RejectsMissingDefaultConnectionString()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:Default", exception.Message, StringComparison.Ordinal);
    }
}
