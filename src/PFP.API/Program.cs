using System.Text;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using PFP.API.Authorization;
using PFP.API.Middleware;
using PFP.Application;
using PFP.Application.Common.Options;
using PFP.Infrastructure;
using PFP.Infrastructure.BackgroundJobs;
using PFP.Infrastructure.Hangfire;
using PFP.Infrastructure.Identity;
using PFP.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase,
                allowIntegerValues: true));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var hangfireConnection = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default is not configured. Set it via appsettings or DATABASE_URL.");

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(hangfireConnection)));

if (!builder.Configuration.GetValue("Hangfire:DisableServer", false))
    builder.Services.AddHangfireServer();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret must be configured (minimum 32 characters).");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

var googleSection = builder.Configuration.GetSection(GoogleOAuthOptions.SectionName);
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];
var googleEnabled = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

if (googleEnabled)
{
    authBuilder
        .AddCookie(GoogleAuthConstants.ExternalCookieScheme, options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = googleClientId!;
            options.ClientSecret = googleClientSecret!;
            options.SignInScheme = GoogleAuthConstants.ExternalCookieScheme;
            options.CallbackPath = "/api/v1/auth/google/callback";
        });
}

builder.Services.Configure<PlatformAdminOptions>(builder.Configuration.GetSection(PlatformAdminOptions.SectionName));
builder.Services.AddSingleton<IAuthorizationHandler, PlatformAdminAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "PlatformAdmin",
        policy => policy.Requirements.Add(new PlatformAdminRequirement()));
});
var app = builder.Build();

if (!app.Configuration.GetValue("Hangfire:DisableRecurringRegistration", false))
{
    // Use IRecurringJobManager (DI) instead of static RecurringJob — JobStorage.Current is not set until
    // Hangfire's ASP.NET Core integration runs; static APIs throw at startup before that happens.
    using var recurringScope = app.Services.CreateScope();
    var recurringJobs = recurringScope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<GenerateBillingCyclesJob>(
        "generate-billing-cycles",
        job => job.Execute(CancellationToken.None),
        "1 0 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobs.AddOrUpdate<CheckOverdueBillingCyclesJob>(
        "check-overdue-billing-cycles",
        job => job.Execute(CancellationToken.None),
        "0 9 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobs.AddOrUpdate<CheckDueInstallmentPaysJob>(
        "check-due-installment-pays",
        job => job.Execute(CancellationToken.None),
        "10 0 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobs.AddOrUpdate<ExecuteAutomationRulesJob>(
        "execute-automation-rules",
        job => job.Execute(CancellationToken.None),
        "0 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobs.AddOrUpdate<ProcessDataExportsJob>(
        "process-data-exports",
        job => job.ProcessNextPendingAsync(CancellationToken.None),
        "*/5 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobs.AddOrUpdate<ExecuteDeletionRequestsJob>(
        "execute-deletion-requests",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 3 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LocalizationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHangfireDashboard(
    "/hangfire",
    new DashboardOptions
    {
        Authorization = new[] { new BasicAuthAuthorizationFilter() },
    });

app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.EnsureAsync(db).ConfigureAwait(false);
}

app.Run();
/// <summary>Marker type for ASP.NET Core MVC integration tests (<c>WebApplicationFactory&lt;Program&gt;</c>).</summary>
public partial class Program { }
