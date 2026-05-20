using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Options;
using PFP.Infrastructure.Gdpr;
using PFP.Infrastructure.Identity;
using PFP.Infrastructure.Persistence;
using PFP.Infrastructure.Persistence.Interceptors;
using PFP.Infrastructure.Services;
using PFP.Infrastructure.Storage;

namespace PFP.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
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
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
        else
            services.AddDistributedMemoryCache();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is not configured. Set it via appsettings or DATABASE_URL.");

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            ConfigureAppDbContext(options, connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<FinanceMonthlyPeriodSeedInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<HistoryInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddDbContextFactory<AppDbContext>(
            options => ConfigureAppDbContext(options, connectionString),
            ServiceLifetime.Scoped);

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddHttpContextAccessor();
        services.AddScoped<IClientRequestContext, HttpClientRequestContext>();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IBalanceCalculator, BalanceCalculator>();
        services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<R2ExportStorageOptions>(configuration.GetSection(R2ExportStorageOptions.SectionName));
        services.AddSingleton<IUserDataExportStorage, R2UserDataExportStorage>();
        services.Configure<R2AttachmentsStorageOptions>(configuration.GetSection(R2AttachmentsStorageOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IStorageService, CloudflareR2StorageService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IAuthEmailDispatcher, NullAuthEmailDispatcher>();
        services.AddSingleton<IDataExportJobScheduler, NoOpDataExportJobScheduler>();

        return services;
    }

    private static void ConfigureAppDbContext(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            sql.EnableRetryOnFailure(maxRetryCount: 3);
        });
    }
}
