# Chuyển dữ liệu SQL Server → PostgreSQL (Neon)

Tool copy toàn bộ bảng ứng dụng từ SQL Server local sang PostgreSQL, dùng cùng model EF Core (snake_case, enum, soft-delete).

## Điều kiện

1. SQL Server local (`EcoSys_Dev`) đang chạy và truy cập được.
2. PostgreSQL (Neon) đã có schema: chạy `dotnet ef database update` hoặc `Database:AutoMigrate` trên API.
3. .NET 8 SDK.

## Bước 1 — Dry-run (xem số dòng, không ghi)

```powershell
cd D:\Litnp\EcoSys\BE\tools\Pfp.DataMigration

$env:PFP_SOURCE_CONNECTION = 'Data Source=PAC163\LITPAC;Initial Catalog=EcoSys_Dev;User ID=sa;Password=123456Aa@;TrustServerCertificate=True'
# Target: lấy từ appsettings.Development.json nếu không set
# $env:PFP_TARGET_CONNECTION = 'Host=...;Database=neondb;...'

dotnet run -- --dry-run
```

## Bước 2 — Migrate thật (xóa dữ liệu Neon rồi copy)

Mặc định **truncate** mọi bảng trên PostgreSQL (trừ `__EFMigrationsHistory`), sau đó copy từ SQL Server.

```powershell
dotnet run
```

Giữ dữ liệu đã có trên Neon và chỉ thêm (có thể lỗi trùng khóa):

```powershell
dotnet run -- --keep-target
```

## Biến môi trường

| Biến | Mô tả |
|------|--------|
| `PFP_SOURCE_CONNECTION` | SQL Server (mặc định: `PAC163\LITPAC` / `EcoSys_Dev`) |
| `PFP_TARGET_CONNECTION` | PostgreSQL / Neon (mặc định: `ConnectionStrings:Default` trong `PFP.API/appsettings.Development.json`) |

## Lưu ý

- Dữ liệu seed trên Neon (admin, categories mặc định) sẽ bị xóa khi chạy migrate không có `--keep-target`.
- Bảng Hangfire (nếu có) không được copy; Hangfire tạo lại khi bật server.
- Neon không cho phép `session_replication_role`; tool xử lý vòng FK giao dịch ↔ trả góp bằng insert hai bước.
- Sau migrate, chạy lại API và đăng nhập bằng tài khoản từ SQL Server (email/password cũ).
