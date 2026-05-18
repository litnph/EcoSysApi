using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Options;
using PFP.Infrastructure.Identity;
using PFP.Infrastructure.BackgroundJobs;
using PFP.Infrastructure.Persistence;
using PFP.Infrastructure.Persistence.Interceptors;
using PFP.Infrastructure.Services;
using PFP.Infrastructure.Storage;

namespace PFP.Infrastructure;

/// <summary>
/// DI bootstrap for the Infrastructure layer. The API project calls
/// <see cref="AddInfrastructure"/> from <c>Program.cs</c> to wire up persistence (and, later,
/// identity / storage / cache / background-jobs modules).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/>, EF Core <c>SaveChanges</c> interceptors, and a Npgsql-backed connection.
    /// Interceptor order: <c>FinanceMonthlyPeriodSeed</c> → <c>SoftDelete</c> → <c>History</c> → <c>Audit</c>.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="configuration">Bound configuration; the connection string is read from <c>ConnectionStrings:Default</c>.</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<HistoryInterceptor>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<FinanceMonthlyPeriodSeedInterceptor>();
        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ISpaceMembershipEvaluator, CachingSpaceMembershipEvaluator>();
        services.AddScoped<ISpaceModuleAccessChecker, NpgsqlSpaceModuleAccessChecker>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:Default is not configured. Set it via appsettings or DATABASE_URL.");

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            });
            options.ReplaceService<IHistoryRepository, SnakeCaseNpgsqlHistoryRepository>();

            // Finance seed runs before soft-delete so new rows get the same SavingChanges pass.
            options.AddInterceptors(
                sp.GetRequiredService<FinanceMonthlyPeriodSeedInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<HistoryInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddHttpContextAccessor();
        services.AddScoped<IClientRequestContext, HttpClientRequestContext>();
        services.AddScoped<AutomationJobEnvironment>();
        services.AddScoped<IAutomationExecutionImpersonation>(sp => sp.GetRequiredService<AutomationJobEnvironment>());
        services.AddScoped<IAutomationTriggerFacts>(sp => sp.GetRequiredService<AutomationJobEnvironment>());
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleOAuthOptions>(configuration.GetSection(GoogleOAuthOptions.SectionName));
        services.Configure<R2ExportStorageOptions>(configuration.GetSection(R2ExportStorageOptions.SectionName));
        services.AddSingleton<IUserDataExportStorage, R2UserDataExportStorage>();
        services.Configure<R2AttachmentsStorageOptions>(configuration.GetSection(R2AttachmentsStorageOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IStorageService, CloudflareR2StorageService>();
        services.AddSingleton<IDataExportJobScheduler, HangfireDataExportJobScheduler>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IAuthEmailDispatcher, LoggingAuthEmailDispatcher>();

        services.AddScoped<GenerateBillingCyclesJob>();
        services.AddScoped<CheckOverdueBillingCyclesJob>();
        services.AddScoped<CheckDueInstallmentPaysJob>();
        services.AddScoped<IAutomationRuleExecutor, AutomationRuleExecutor>();
        services.AddScoped<ExecuteAutomationRulesJob>();
        services.AddScoped<ProcessDataExportsJob>();
        services.AddScoped<ExecuteDeletionRequestsJob>();

        return services;
    }
}
