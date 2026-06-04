# EF Core migrations (PostgreSQL)

Migrations target **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`. SQL Server migration history was removed when switching providers.

## Generate a new migration

From the repository `BE` folder, with **Visual Studio not debugging PFP.API** (otherwise `bin\Debug` DLLs may be locked):

```powershell
cd D:\Litnp\EcoSys\BE
$env:PFP_DESIGN_CONNECTION = 'Host=localhost;Port=5432;Database=pfp_ef_design;Username=postgres;Password=postgres'

dotnet ef migrations add <MigrationName> `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext `
  --output-dir Persistence\Migrations
```

## Apply schema

Set `ConnectionStrings:Default` in `appsettings.Development.json` (or `PFP_DESIGN_CONNECTION` / `DATABASE_URL`), then:

```powershell
dotnet ef database update `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext
```

With `Database:AutoMigrate` enabled, the API applies pending migrations on startup.

Hangfire will create its own tables in the same database on first API startup when the Hangfire server is enabled.
