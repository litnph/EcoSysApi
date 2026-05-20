# EF Core migrations (SQL Server)

Migrations for this project target **SQL Server** (provider `Microsoft.EntityFrameworkCore.SqlServer`). The previous PostgreSQL migration history was removed as part of the provider switch.

## Generate the initial migration

From the repository `BE` folder, with **Visual Studio not debugging PFP.API** (otherwise `bin\Debug` DLLs may be locked):

```powershell
cd D:\Litnp\EcoSys\BE
# Optional: avoid reading appsettings.Development.json during design-time
$env:PFP_DESIGN_CONNECTION = 'Server=(localdb)\mssqllocaldb;Database=pfp_ef_design;Trusted_Connection=True;TrustServerCertificate=True'

dotnet ef migrations add InitialSqlServer `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext `
  --output-dir Persistence\Migrations
```

## Apply schema to your server

Ensure the database in `ConnectionStrings:Default` exists (e.g. `EcoSys` on `PAC163\LITPAC`), then:

```powershell
dotnet ef database update `
  --project src\PFP.Infrastructure\PFP.Infrastructure.csproj `
  --startup-project src\PFP.API\PFP.API.csproj `
  --context AppDbContext
```

Hangfire will create its own tables in the same database on first API startup.
