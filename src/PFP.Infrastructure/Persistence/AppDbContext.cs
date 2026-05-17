using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Infrastructure.Persistence.Configurations.Common;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Single application <see cref="DbContext"/> wiring every entity in the platform.
/// <para>
/// Configures, in one place (per spec §3.1 / §3.2 / §3.3):
/// <list type="bullet">
/// <item><b>DbSets</b> for every entity across Layer 0 (identity / i18n / audit), Layer 1 (platform
/// core), Layer 2 (finance), and the corresponding <c>*_HISTORY</c> tables.</item>
/// <item><b>Snake_case naming</b> for tables, columns, keys, FKs, and indexes — matching the
/// upper-case-snake schema names used by the spec (<c>FIN_TRANSACTIONS</c> ↔ <c>fin_transactions</c>).</item>
/// <item><b>Global query filter</b> <c>WHERE is_deleted = false</c> on every
/// <see cref="SoftDeletableEntity"/> so soft-deleted rows are invisible to ordinary reads.</item>
/// <item><b>Enum → snake_case string conversion</b> applied globally to every enum in
/// <c>PFP.Domain.Enums</c> through <see cref="SnakeCaseEnumConverter{TEnum}"/>.</item>
/// <item><b>Per-entity configurations</b> picked up via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>
/// (one <see cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{TEntity}"/> per table, in the
/// <c>Configurations/</c> tree).</item>
/// </list>
/// </para>
/// <para>
/// The three EF Core interceptors (<see cref="Interceptors.SoftDeleteInterceptor"/>,
/// <see cref="Interceptors.HistoryInterceptor"/>, <see cref="Interceptors.AuditInterceptor"/>) are
/// added to the <c>DbContextOptions</c> in <see cref="InfrastructureServiceCollectionExtensions.AddInfrastructure"/>
/// so they run on every <c>SaveChanges</c> in the order required by the spec
/// (SoftDelete → History → Audit).
/// </para>
/// </summary>
public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    /// <summary>Creates the context. The supplied options must already carry the registered interceptors.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ───────────────────────── Layer 0 — Identity & Profile ─────────────────────────

    public DbSet<User> Users => Set<User>();
    public DbSet<UserAuthProvider> UserAuthProviders => Set<UserAuthProvider>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserLoginAttempt> UserLoginAttempts => Set<UserLoginAttempt>();
    public DbSet<UserPasswordReset> UserPasswordResets => Set<UserPasswordReset>();
    public DbSet<UserEmailVerification> UserEmailVerifications => Set<UserEmailVerification>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAvatarUpload> UserAvatarUploads => Set<UserAvatarUpload>();
    public DbSet<UserNotificationPref> UserNotificationPrefs => Set<UserNotificationPref>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserDataExport> UserDataExports => Set<UserDataExport>();
    public DbSet<UserDeletionRequest> UserDeletionRequests => Set<UserDeletionRequest>();

    // ───────────────────────── Layer 0 — i18n ─────────────────────────

    public DbSet<Locale> Locales => Set<Locale>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<TranslationFallback> TranslationFallbacks => Set<TranslationFallback>();
    public DbSet<UIString> UIStrings => Set<UIString>();

    // ───────────────────────── Layer 0 — Audit ─────────────────────────

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemEventLog> SystemEventLogs => Set<SystemEventLog>();
    public DbSet<AuditLogRetention> AuditLogRetentions => Set<AuditLogRetention>();

    // ───────────────────────── Layer 1 — Platform Core ─────────────────────────

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrgMember> OrgMembers => Set<OrgMember>();
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<SpaceMember> SpaceMembers => Set<SpaceMember>();
    public DbSet<SpaceModule> SpaceModules => Set<SpaceModule>();

    /// <summary>Finance automation rules (<c>automation_rules</c>).</summary>
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();

    /// <summary>Automation execution audit (<c>automation_logs</c>).</summary>
    public DbSet<AutomationLog> AutomationLogs => Set<AutomationLog>();

    /// <summary>Platform feature toggles (<c>feature_flags</c>).</summary>
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    /// <summary>Per-user / per-org feature flag overrides (<c>feature_flag_overrides</c>).</summary>
    public DbSet<FeatureFlagOverride> FeatureFlagOverrides => Set<FeatureFlagOverride>();

    /// <summary>Uploaded file blobs (<c>file_attachments</c>).</summary>
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    /// <summary>Finance-scoped taxonomy labels (<c>tags</c>).</summary>
    public DbSet<Tag> Tags => Set<Tag>();

    /// <summary>Polymorphic tag applications (<c>entity_tags</c>).</summary>
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();

    /// <summary>Polymorphic threaded comments (<c>comments</c>).</summary>
    public DbSet<Comment> Comments => Set<Comment>();

    // ───────────────────────── Layer 1 — Platform History ─────────────────────────

    public DbSet<OrganizationHistory> OrganizationHistory => Set<OrganizationHistory>();
    public DbSet<OrgMemberHistory> OrgMemberHistory => Set<OrgMemberHistory>();
    public DbSet<SpaceHistory> SpaceHistory => Set<SpaceHistory>();
    public DbSet<SpaceMemberHistory> SpaceMemberHistory => Set<SpaceMemberHistory>();

    // ───────────────────────── Layer 2 — Finance ─────────────────────────

    public DbSet<FinSource> FinSources => Set<FinSource>();
    public DbSet<FinCategory> FinCategories => Set<FinCategory>();
    public DbSet<FinTransaction> FinTransactions => Set<FinTransaction>();
    public DbSet<FinTxnSplit> FinTxnSplits => Set<FinTxnSplit>();
    public DbSet<FinBillingCycle> FinBillingCycles => Set<FinBillingCycle>();
    public DbSet<FinMonthlyPeriod> FinMonthlyPeriods => Set<FinMonthlyPeriod>();
    public DbSet<FinInstallmentPlan> FinInstallmentPlans => Set<FinInstallmentPlan>();
    public DbSet<FinInstallmentPay> FinInstallmentPays => Set<FinInstallmentPay>();
    public DbSet<FinDebtRecord> FinDebtRecords => Set<FinDebtRecord>();
    public DbSet<FinDebtTransaction> FinDebtTransactions => Set<FinDebtTransaction>();
    public DbSet<FinSaving> FinSavings => Set<FinSaving>();
    public DbSet<FinInvestment> FinInvestments => Set<FinInvestment>();
    public DbSet<FinInvestmentTxn> FinInvestmentTxns => Set<FinInvestmentTxn>();

    // ───────────────────────── Layer 2 — Finance History ─────────────────────────

    public DbSet<FinTransactionHistory> FinTransactionHistory => Set<FinTransactionHistory>();
    public DbSet<FinSourceHistory> FinSourceHistory => Set<FinSourceHistory>();
    public DbSet<FinDebtRecordHistory> FinDebtRecordHistory => Set<FinDebtRecordHistory>();
    public DbSet<FinInstallmentPlanHistory> FinInstallmentPlanHistory => Set<FinInstallmentPlanHistory>();

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Forensic / audit tables (UserSession, UserProfile, UserAuthProvider, …, every *_HISTORY,
        // FIN_DEBT_TRANSACTIONS, FIN_INVESTMENT_TXN) intentionally do NOT carry a soft-delete filter
        // even when their parent does. When the parent is soft-deleted these dependent rows remain
        // visible — that is by design (cf. spec §2 "Audit trail đầy đủ" / "Soft delete toàn bộ").
        // EF Core flags this as a potential foot-gun; we acknowledge it once here.
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
    }

    /// <inheritdoc/>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Default every decimal column to numeric(18,2) — VND amounts are integers, finance amounts
        // never exceed 16 digits left of the point. Specific columns override this in their config.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

        // Register a snake_case string converter for every enum in PFP.Domain.Enums so that, regardless
        // of where the enum first appears in the model graph, it is persisted human-readably.
        var enumTypes = typeof(BaseEntity).Assembly
            .GetTypes()
            .Where(t => t.IsEnum && t.Namespace == "PFP.Domain.Enums");

        foreach (var enumType in enumTypes)
        {
            var converterType = typeof(SnakeCaseEnumConverter<>).MakeGenericType(enumType);
            configurationBuilder.Properties(enumType).HaveConversion(converterType);
        }
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Pull in every IEntityTypeConfiguration in this assembly (one per table per spec §2.2).
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // 2. Install the soft-delete global query filter on every SoftDeletableEntity (spec §3.2).
        builder.ApplySoftDeleteQueryFilter();

        // 3. Bake static seed data (locales, default UI strings) into the migration via HasData.
        //    Seeded BEFORE the snake_case pass so the generated SQL matches the live column names.
        DbInitializer.ApplyModelSeed(builder);

        // 4. Snake_case every table / column / key / fk / index for clean PostgreSQL DDL.
        //    Must run last so it catches names introduced by ApplyConfigurations.
        builder.ApplySnakeCaseNaming();
    }
}
