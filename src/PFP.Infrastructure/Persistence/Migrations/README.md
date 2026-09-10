# EF Core migrations (SQL Server)

Migrations target **SQL Server** via `Microsoft.EntityFrameworkCore.SqlServer`.
PostgreSQL/Npgsql migration history was removed when switching providers.

## Generate a new migration

From the repository `EcoSysApi` folder, with **Visual Studio not debugging PFP.API** (otherwise `bin\Debug` DLLs may be locked):

```powershell
cd D:\Git\EcoSys\EcoSysApi

dotnet ef migrations add <MigrationName> `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext `
  --output-dir Persistence\Migrations
```

## Apply schema

Set `ConnectionStrings:Default` in `appsettings.Development.json`. For an ad-hoc override,
use the standard `ConnectionStrings__Default` environment variable. Then run:

```powershell
dotnet ef database update `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext
```

With `Database:AutoMigrate` enabled, the API applies pending migrations on startup.
