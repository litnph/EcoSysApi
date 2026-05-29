using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Infrastructure.Persistence.Configurations.Common;

namespace PFP.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Locale> Locales => Set<Locale>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<TranslationFallback> TranslationFallbacks => Set<TranslationFallback>();
    public DbSet<UIString> UIStrings => Set<UIString>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemEventLog> SystemEventLogs => Set<SystemEventLog>();

    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();

    public DbSet<FinSource> FinSources => Set<FinSource>();
    public DbSet<FinCategory> FinCategories => Set<FinCategory>();
    public DbSet<FinTransaction> FinTransactions => Set<FinTransaction>();
    public DbSet<FinTxnSplit> FinTxnSplits => Set<FinTxnSplit>();
    public DbSet<FinBillingCycle> FinBillingCycles => Set<FinBillingCycle>();
    public DbSet<FinBillingCycleItem> FinBillingCycleItems => Set<FinBillingCycleItem>();
    public DbSet<FinMonthlyPeriod> FinMonthlyPeriods => Set<FinMonthlyPeriod>();
    public DbSet<FinInstallmentPlan> FinInstallmentPlans => Set<FinInstallmentPlan>();
    public DbSet<FinInstallmentPay> FinInstallmentPays => Set<FinInstallmentPay>();
    public DbSet<FinDebtRecord> FinDebtRecords => Set<FinDebtRecord>();
    public DbSet<FinDebtTransaction> FinDebtTransactions => Set<FinDebtTransaction>();
    public DbSet<FinSaving> FinSavings => Set<FinSaving>();
    public DbSet<FinInvestment> FinInvestments => Set<FinInvestment>();
    public DbSet<FinInvestmentTxn> FinInvestmentTxns => Set<FinInvestmentTxn>();

    public DbSet<FinTransactionHistory> FinTransactionHistory => Set<FinTransactionHistory>();
    public DbSet<FinSourceHistory> FinSourceHistory => Set<FinSourceHistory>();
    public DbSet<FinDebtRecordHistory> FinDebtRecordHistory => Set<FinDebtRecordHistory>();
    public DbSet<FinInstallmentPlanHistory> FinInstallmentPlanHistory => Set<FinInstallmentPlanHistory>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

        var enumTypes = typeof(BaseEntity).Assembly
            .GetTypes()
            .Where(t => t.IsEnum && t.Namespace == "PFP.Domain.Enums");

        foreach (var enumType in enumTypes)
        {
            var converterType = typeof(SnakeCaseEnumConverter<>).MakeGenericType(enumType);
            configurationBuilder.Properties(enumType).HaveConversion(converterType);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilter();
        DbInitializer.ApplyModelSeed(builder);
        builder.ApplySnakeCaseNaming();
    }
}
