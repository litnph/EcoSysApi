using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PFP.API.Authorization;
using PFP.API.Configuration;
using PFP.API.Filters;
using PFP.API.Middleware;
using PFP.Application;
using PFP.Application.Common.Options;
using PFP.Infrastructure;
using PFP.Infrastructure.Identity;
using PFP.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddRenderDatabaseUrl();
builder.AddFrontendCors();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase,
                allowIntegerValues: false));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPfpSwagger();
builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPlatformRateLimiting();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtSecret = jwtSection["Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret)
    || jwtSecret.Length < 32
    || jwtSecret.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Jwt:Secret must be a non-placeholder secret of at least 32 characters.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(options =>
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

builder.Services.Configure<PlatformAdminOptions>(builder.Configuration.GetSection(PlatformAdminOptions.SectionName));
builder.Services.AddSingleton<IAuthorizationHandler, PlatformAdminAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdmin", policy => policy.Requirements.Add(new PlatformAdminRequirement()));
});

var app = builder.Build();

// Render terminates TLS at its edge. Resolve the original scheme/client before
// any middleware reads request metadata or performs HTTPS redirection.
app.UseProductionProxy();
// Keep the Render liveness probe independent from authentication, localisation,
// and database availability. Readiness is exercised by the normal API routes.
app.UseHealthChecks("/health");

if (!app.Environment.IsProduction()
    && (app.Configuration.GetValue("Database:AutoMigrate", false)
        || app.Configuration.GetValue("Database:RunSeedOnStartup", false)))
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Configuration.GetValue("Database:AutoMigrate", false))
    {
        startupLogger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync().ConfigureAwait(false);
    }

    if (app.Configuration.GetValue("Database:RunSeedOnStartup", false))
    {
        startupLogger.LogInformation("Seeding database...");
        await DbInitializer.EnsureAsync(db).ConfigureAwait(false);
        await AdminUserSeeder.EnsureAsync(
            db,
            app.Configuration,
            scope.ServiceProvider.GetRequiredService<PFP.Application.Common.Interfaces.IPasswordHasher>(),
            CancellationToken.None).ConfigureAwait(false);
        if (app.Configuration.GetValue("Seed:DemoFinance:Reset", false))
        {
            startupLogger.LogInformation("Resetting demo finance data...");
        }

        await ExpenseCategorySeeder.EnsureAsync(db, app.Configuration, CancellationToken.None).ConfigureAwait(false);
        await IncomeCategorySeeder.EnsureAsync(db, app.Configuration, CancellationToken.None).ConfigureAwait(false);
        await TransferCategorySeeder.EnsureAsync(db, app.Configuration, CancellationToken.None).ConfigureAwait(false);
        await DemoFinanceSeeder.EnsureAsync(db, app.Configuration, CancellationToken.None).ConfigureAwait(false);
        startupLogger.LogInformation("Database seed completed.");
    }
}

app.UsePfpMiddleware();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next().ConfigureAwait(false);
});
if (app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Swagger:Enabled", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseFrontendCors();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new { service = "PFP.API", status = "healthy" }))
    .AllowAnonymous();
app.MapControllers();
app.Run();

public partial class Program;
