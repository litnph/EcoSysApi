using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<UserLoginAttempt> UserLoginAttempts { get; }
    DbSet<UserAvatarUpload> UserAvatarUploads { get; }
    DbSet<UserNotificationPref> UserNotificationPrefs { get; }
    DbSet<UserDataExport> UserDataExports { get; }
    DbSet<UserDeletionRequest> UserDeletionRequests { get; }
    DbSet<Notification> Notifications { get; }

    DbSet<AutomationRule> AutomationRules { get; }
    DbSet<AutomationLog> AutomationLogs { get; }
    DbSet<FeatureFlag> FeatureFlags { get; }
    DbSet<FeatureFlagOverride> FeatureFlagOverrides { get; }
    DbSet<FileAttachment> FileAttachments { get; }
    DbSet<Tag> Tags { get; }
    DbSet<EntityTag> EntityTags { get; }
    DbSet<Comment> Comments { get; }

    DbSet<FinSource> FinSources { get; }
    DbSet<FinCategory> FinCategories { get; }
    DbSet<FinMonthlyPeriod> FinMonthlyPeriods { get; }
    DbSet<FinTransaction> FinTransactions { get; }
    DbSet<FinTxnSplit> FinTxnSplits { get; }
    DbSet<FinDebtRecord> FinDebtRecords { get; }
    DbSet<FinDebtTransaction> FinDebtTransactions { get; }
    DbSet<FinDebtRecordHistory> FinDebtRecordHistory { get; }
    DbSet<FinBillingCycle> FinBillingCycles { get; }
    DbSet<FinInstallmentPlan> FinInstallmentPlans { get; }
    DbSet<FinInstallmentPay> FinInstallmentPays { get; }
    DbSet<FinTransactionHistory> FinTransactionHistory { get; }
    DbSet<FinSourceHistory> FinSourceHistory { get; }
    DbSet<FinInstallmentPlanHistory> FinInstallmentPlanHistory { get; }
    DbSet<FinSaving> FinSavings { get; }
    DbSet<FinInvestment> FinInvestments { get; }
    DbSet<FinInvestmentTxn> FinInvestmentTxns { get; }

    DbSet<SystemEventLog> SystemEventLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Locale> Locales { get; }
    DbSet<Translation> Translations { get; }
    DbSet<TranslationFallback> TranslationFallbacks { get; }
    DbSet<UIString> UIStrings { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
