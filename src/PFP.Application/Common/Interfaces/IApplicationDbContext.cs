using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Application-layer abstraction over the EF Core <see cref="DbContext"/> so MediatR handlers stay
/// persistence-agnostic while still participating in explicit transactions.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Users table (<c>USERS</c>).</summary>
    DbSet<User> Users { get; }

    /// <summary>User profiles (<c>USER_PROFILES</c>).</summary>
    DbSet<UserProfile> UserProfiles { get; }

    /// <summary>Linked auth providers (<c>USER_AUTH_PROVIDERS</c>).</summary>
    DbSet<UserAuthProvider> UserAuthProviders { get; }

    /// <summary>Organisations (<c>ORGANIZATIONS</c>).</summary>
    DbSet<Organization> Organizations { get; }

    /// <summary>Organisation memberships (<c>ORG_MEMBERS</c>).</summary>
    DbSet<OrgMember> OrgMembers { get; }

    /// <summary>Spaces (<c>SPACES</c>).</summary>
    DbSet<Space> Spaces { get; }

    /// <summary>Space modules (<c>SPACE_MODULES</c>).</summary>
    DbSet<SpaceModule> SpaceModules { get; }

    /// <summary>Space memberships (<c>SPACE_MEMBERS</c>).</summary>
    DbSet<SpaceMember> SpaceMembers { get; }

    /// <summary>Finance automation rules (<c>automation_rules</c>).</summary>
    DbSet<AutomationRule> AutomationRules { get; }

    /// <summary>Automation audit log (<c>automation_logs</c>).</summary>
    DbSet<AutomationLog> AutomationLogs { get; }

    /// <summary>Platform feature toggles (<c>feature_flags</c>).</summary>
    DbSet<FeatureFlag> FeatureFlags { get; }

    /// <summary>Per-user / per-org feature flag overrides (<c>feature_flag_overrides</c>).</summary>
    DbSet<FeatureFlagOverride> FeatureFlagOverrides { get; }

    /// <summary>Uploaded file attachments (<c>file_attachments</c>).</summary>
    DbSet<FileAttachment> FileAttachments { get; }

    /// <summary>Finance taxonomy tags (<c>tags</c>).</summary>
    DbSet<Tag> Tags { get; }

    /// <summary>Entity ↔ tag junction (<c>entity_tags</c>).</summary>
    DbSet<EntityTag> EntityTags { get; }

    /// <summary>Threaded entity comments (<c>comments</c>).</summary>
    DbSet<Comment> Comments { get; }

    /// <summary>Finance sources (<c>FIN_SOURCES</c>).</summary>
    DbSet<FinSource> FinSources { get; }

    /// <summary>Finance categories (<c>FIN_CATEGORIES</c>).</summary>
    DbSet<FinCategory> FinCategories { get; }

    /// <summary>Finance monthly periods (<c>FIN_MONTHLY_PERIODS</c>).</summary>
    DbSet<FinMonthlyPeriod> FinMonthlyPeriods { get; }

    /// <summary>Finance transactions (<c>FIN_TRANSACTIONS</c>).</summary>
    DbSet<FinTransaction> FinTransactions { get; }

    /// <summary>Split-payment participant rows (<c>fin_txn_splits</c>).</summary>
    DbSet<FinTxnSplit> FinTxnSplits { get; }

    /// <summary>Debt / loan records (<c>fin_debt_records</c>).</summary>
    DbSet<FinDebtRecord> FinDebtRecords { get; }

    /// <summary>Append-only debt ledger (<c>fin_debt_transactions</c>).</summary>
    DbSet<FinDebtTransaction> FinDebtTransactions { get; }

    /// <summary>Debt record version history (<c>fin_debt_record_history</c>).</summary>
    DbSet<FinDebtRecordHistory> FinDebtRecordHistory { get; }

    /// <summary>Credit-card billing cycles (<c>FIN_BILLING_CYCLES</c>).</summary>
    DbSet<FinBillingCycle> FinBillingCycles { get; }

    /// <summary>Installment plans (<c>fin_installment_plans</c>).</summary>
    DbSet<FinInstallmentPlan> FinInstallmentPlans { get; }

    /// <summary>Installment schedule rows (<c>fin_installment_pays</c>).</summary>
    DbSet<FinInstallmentPay> FinInstallmentPays { get; }

    /// <summary>Append-only system / job diagnostics (<c>SYSTEM_EVENT_LOGS</c>).</summary>
    DbSet<SystemEventLog> SystemEventLogs { get; }

    /// <summary>In-app user notifications (<c>notifications</c>).</summary>
    DbSet<Notification> Notifications { get; }

    /// <summary>Finance transaction version history (<c>fin_transaction_history</c>).</summary>
    DbSet<FinTransactionHistory> FinTransactionHistory { get; }

    /// <summary>Savings goals / books (<c>fin_savings</c>).</summary>
    DbSet<FinSaving> FinSavings { get; }

    /// <summary>Investment positions (<c>fin_investments</c>).</summary>
    DbSet<FinInvestment> FinInvestments { get; }

    /// <summary>Append-only investment ledger (<c>fin_investment_txns</c>).</summary>
    DbSet<FinInvestmentTxn> FinInvestmentTxns { get; }

    /// <summary>User audit trail (<c>AUDIT_LOGS</c>).</summary>
    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>Email verification tokens (<c>USER_EMAIL_VERIFICATIONS</c>).</summary>
    DbSet<UserEmailVerification> UserEmailVerifications { get; }

    /// <summary>Password-reset tokens (<c>USER_PASSWORD_RESETS</c>).</summary>
    DbSet<UserPasswordReset> UserPasswordResets { get; }

    /// <summary>Refresh-token sessions (<c>USER_SESSIONS</c>).</summary>
    DbSet<UserSession> UserSessions { get; }

    /// <summary>GDPR data-export jobs (<c>USER_DATA_EXPORTS</c>).</summary>
    DbSet<UserDataExport> UserDataExports { get; }

    /// <summary>GDPR deletion flow (<c>USER_DELETION_REQUESTS</c>).</summary>
    DbSet<UserDeletionRequest> UserDeletionRequests { get; }

    /// <summary>Login attempts (<c>USER_LOGIN_ATTEMPTS</c>).</summary>
    DbSet<UserLoginAttempt> UserLoginAttempts { get; }

    /// <summary>Avatar upload history (<c>USER_AVATAR_UPLOADS</c>).</summary>
    DbSet<UserAvatarUpload> UserAvatarUploads { get; }

    /// <summary>Granular notification preferences (<c>USER_NOTIFICATION_PREFS</c>).</summary>
    DbSet<UserNotificationPref> UserNotificationPrefs { get; }

    /// <summary>Per-entity audit retention policies (<c>AUDIT_LOG_RETENTIONS</c>).</summary>
    DbSet<AuditLogRetention> AuditLogRetentions { get; }

    /// <summary>Supported locales (<c>LOCALES</c>).</summary>
    DbSet<Locale> Locales { get; }

    /// <summary>Polymorphic entity field translations (<c>TRANSLATIONS</c>).</summary>
    DbSet<Translation> Translations { get; }

    /// <summary>Per-locale fallback chains (<c>TRANSLATION_FALLBACKS</c>).</summary>
    DbSet<TranslationFallback> TranslationFallbacks { get; }

    /// <summary>Admin-managed UI labels (<c>UI_STRINGS</c>).</summary>
    DbSet<UIString> UIStrings { get; }

    /// <summary>Low-level EF helpers (transactions, execution strategy, …).</summary>
    DatabaseFacade Database { get; }

    /// <summary>Persists pending changes tracked by EF Core.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
