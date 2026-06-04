using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Infrastructure.Persistence;

// SQL Server DateTime is Unspecified; allow writing to PostgreSQL timestamptz during one-off migration.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

const string defaultSource =
    "Data Source=PAC163\\LITPAC;Initial Catalog=EcoSys_Dev;User ID=sa;Password=123456Aa@;TrustServerCertificate=True";

var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
var keepTarget = args.Contains("--keep-target", StringComparer.OrdinalIgnoreCase);

var sourceCs = Environment.GetEnvironmentVariable("PFP_SOURCE_CONNECTION") ?? defaultSource;
var targetCs = Environment.GetEnvironmentVariable("PFP_TARGET_CONNECTION")
               ?? TryReadTargetFromApiAppsettings()
               ?? throw new InvalidOperationException(
                   "Set PFP_TARGET_CONNECTION (PostgreSQL / Neon) or configure ConnectionStrings:Default in PFP.API appsettings.Development.json.");

Console.WriteLine("PFP data migration: SQL Server -> PostgreSQL");
Console.WriteLine($"  Source: {MaskConnection(sourceCs)}");
Console.WriteLine($"  Target: {MaskConnection(targetCs)}");
Console.WriteLine($"  Mode:   {(dryRun ? "dry-run (no writes)" : keepTarget ? "merge (no truncate)" : "replace (truncate target first)")}");
Console.WriteLine();

await using var source = CreateContext(sourceCs, useSqlServer: true);
await using var target = CreateContext(targetCs, useSqlServer: false);

if (!await source.Database.CanConnectAsync())
    throw new InvalidOperationException("Cannot connect to SQL Server source. Check PFP_SOURCE_CONNECTION and that the server is reachable.");

if (!await target.Database.CanConnectAsync())
    throw new InvalidOperationException("Cannot connect to PostgreSQL target. Check PFP_TARGET_CONNECTION.");

if (!dryRun && !keepTarget)
    await TruncatePublicTablesAsync(targetCs);

var total = 0;
total += await CopyAsync<AuditLogRetention>(source, target, dryRun);
total += await CopyAsync<Locale>(source, target, dryRun);
total += await CopyAsync<FinCategory>(source, target, dryRun);
total += await CopyAsync<User>(source, target, dryRun);
total += await CopyAsync<UserProfile>(source, target, dryRun);
total += await CopyAsync<UserSession>(source, target, dryRun);
total += await CopyAsync<TranslationFallback>(source, target, dryRun);
total += await CopyAsync<Translation>(source, target, dryRun);
total += await CopyAsync<UIString>(source, target, dryRun);
total += await CopyAsync<Tag>(source, target, dryRun);
total += await CopyAsync<FinSource>(source, target, dryRun);
total += await CopyAsync<FinInvestment>(source, target, dryRun);
total += await CopyAsync<FinSaving>(source, target, dryRun);
total += await CopyAsync<FinMonthlyPeriod>(source, target, dryRun);
total += await CopyTransactionsAsync(source, target, dryRun);
total += await CopyAsync<FinInstallmentPlan>(source, target, dryRun);
if (!dryRun)
    await RestoreTransactionInstallmentPlanIdsAsync(source, targetCs);
total += await CopyAsync<FinBillingCycle>(source, target, dryRun);
total += await CopyAsync<FinBillingCycleItem>(source, target, dryRun);
total += await CopyAsync<FinInstallmentPay>(source, target, dryRun);
total += await CopyAsync<FinTxnSplit>(source, target, dryRun);
total += await CopyAsync<FinDebtRecord>(source, target, dryRun);
total += await CopyAsync<FinDebtTransaction>(source, target, dryRun);
total += await CopyAsync<FinInvestmentTxn>(source, target, dryRun);
total += await CopyAsync<EntityTag>(source, target, dryRun);
total += await CopyAsync<FileAttachment>(source, target, dryRun);
total += await CopyAsync<AuditLog>(source, target, dryRun);
total += await CopyAsync<SystemEventLog>(source, target, dryRun);
total += await CopyAsync<FinTransactionHistory>(source, target, dryRun);
total += await CopyAsync<FinSourceHistory>(source, target, dryRun);
total += await CopyAsync<FinDebtRecordHistory>(source, target, dryRun);
total += await CopyAsync<FinInstallmentPlanHistory>(source, target, dryRun);

Console.WriteLine();
Console.WriteLine(dryRun
    ? $"Dry-run complete. {total} rows would be copied."
    : $"Migration complete. {total} rows copied.");

static AppDbContext CreateContext(string connectionString, bool useSqlServer)
{
    var options = new DbContextOptionsBuilder<AppDbContext>();
    if (useSqlServer)
    {
        options.UseSqlServer(connectionString, sql =>
            sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
    }
    else
    {
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
    }

    return new AppDbContext(options.Options);
}

static async Task<int> CopyAsync<T>(AppDbContext source, AppDbContext target, bool dryRun)
    where T : class
{
    var items = await source.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync();
    Console.WriteLine($"  {typeof(T).Name,-32} {items.Count,6} rows");

    if (dryRun || items.Count == 0)
        return items.Count;

    await target.Set<T>().AddRangeAsync(items);
    await target.SaveChangesAsync();
    target.ChangeTracker.Clear();
    return items.Count;
}

/// <summary>
/// Breaks the transaction &lt;-&gt; installment-plan cycle: insert txns without plan FK, then plans, then restore FK.
/// </summary>
static async Task<int> CopyTransactionsAsync(AppDbContext source, AppDbContext target, bool dryRun)
{
    var items = await source.FinTransactions.IgnoreQueryFilters().AsNoTracking().ToListAsync();
    Console.WriteLine($"  {"FinTransaction",-32} {items.Count,6} rows");

    if (dryRun || items.Count == 0)
        return items.Count;

    foreach (var txn in items)
        txn.InstallmentPlanId = null;

    await target.FinTransactions.AddRangeAsync(items);
    await target.SaveChangesAsync();
    target.ChangeTracker.Clear();
    return items.Count;
}

static async Task RestoreTransactionInstallmentPlanIdsAsync(AppDbContext source, string targetCs)
{
    var links = await source.FinTransactions
        .IgnoreQueryFilters()
        .AsNoTracking()
        .Where(t => t.InstallmentPlanId != null)
        .Select(t => new { t.Id, t.InstallmentPlanId })
        .ToListAsync();

    if (links.Count == 0)
        return;

    await using var conn = new NpgsqlConnection(targetCs);
    await conn.OpenAsync();
    await using var tx = await conn.BeginTransactionAsync();
    await using var cmd = conn.CreateCommand();
    cmd.Transaction = tx;
    cmd.CommandText = "UPDATE fin_transactions SET installment_plan_id = @plan_id WHERE id = @id";

    var planParam = cmd.Parameters.Add("plan_id", NpgsqlTypes.NpgsqlDbType.Uuid);
    var idParam = cmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Uuid);

    foreach (var link in links)
    {
        idParam.Value = link.Id;
        planParam.Value = link.InstallmentPlanId!;
        await cmd.ExecuteNonQueryAsync();
    }

    await tx.CommitAsync();
    Console.WriteLine($"  {"FinTransaction (FK restore)",-32} {links.Count,6} rows");
}

static async Task TruncatePublicTablesAsync(string connectionString)
{
    Console.WriteLine("Truncating PostgreSQL tables (except __EFMigrationsHistory)...");
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText =
        """
        DO $$
        DECLARE r RECORD;
        BEGIN
          FOR r IN
            SELECT tablename
              FROM pg_tables
             WHERE schemaname = 'public'
               AND tablename <> '__EFMigrationsHistory'
          LOOP
            EXECUTE format('TRUNCATE TABLE %I RESTART IDENTITY CASCADE', r.tablename);
          END LOOP;
        END $$;
        """;
    await cmd.ExecuteNonQueryAsync();
}

static string? TryReadTargetFromApiAppsettings()
{
    var roots = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

    foreach (var root in roots)
    {
        var dir = new DirectoryInfo(root);
        for (var depth = 0; depth < 12 && dir != null; depth++, dir = dir.Parent!)
        {
            var path = Path.Combine(dir.FullName, "src", "PFP.API", "appsettings.Development.json");
            if (!File.Exists(path)) continue;

            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var cs)) continue;
                if (!cs.TryGetProperty("Default", out var def)) continue;
                return def.GetString();
            }
            catch (JsonException)
            {
                // try next path
            }
        }
    }

    return null;
}

static string MaskConnection(string cs)
{
    var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return string.Join(';', parts.Select(p =>
        p.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ? "Password=***" : p));
}
