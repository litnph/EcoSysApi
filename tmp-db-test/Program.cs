using Npgsql;

var cs =
    "Host=ep-autumn-sound-aorl1h79-pooler.c-2.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_uL5Vl6KEiTxz;SSL Mode=Require;Channel Binding=Require;Timeout=15;Command Timeout=15";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();

await using var cmd = new NpgsqlCommand(
    """
    SELECT u.email, u.password_hash, o.id AS org_id
    FROM users u
    LEFT JOIN organizations o ON o.owner_id = u.id AND o.is_personal = true
    WHERE u.email = 'litnph@gmail.com'
    """,
    conn);
await using var r = await cmd.ExecuteReaderAsync();
while (await r.ReadAsync())
{
    var hash = r.IsDBNull(1) ? null : r.GetString(1);
    var orgId = r.IsDBNull(2) ? (Guid?)null : r.GetGuid(2);
    var pwOk = hash is not null && BCrypt.Net.BCrypt.Verify("La23111999@", hash);
    Console.WriteLine($"email={r.GetString(0)} org={orgId} passwordOk={pwOk}");
}
