using Microsoft.Data.SqlClient;

var cs = Environment.GetEnvironmentVariable("PFP_TMP_CONNECTION")
         ?? throw new InvalidOperationException(
             "Set env PFP_TMP_CONNECTION to your SQL Server connection string (same as ConnectionStrings:Default).");

var email = Environment.GetEnvironmentVariable("PFP_TMP_EMAIL")
            ?? throw new InvalidOperationException("Set env PFP_TMP_EMAIL to the user email to inspect.");

var plainPassword = Environment.GetEnvironmentVariable("PFP_TMP_PASSWORD_PLAIN");

await using var conn = new SqlConnection(cs);
await conn.OpenAsync();

await using var cmd = conn.CreateCommand();
cmd.CommandText =
    """
    SELECT TOP (1) u.email, u.password_hash, o.id AS org_id
      FROM users u
      LEFT JOIN organizations o ON o.owner_id = u.id AND o.is_personal = 1
     WHERE u.email = @email
    """;

var p = cmd.CreateParameter();
p.ParameterName = "@email";
p.Value = email;
cmd.Parameters.Add(p);

await using var r = await cmd.ExecuteReaderAsync();
while (await r.ReadAsync())
{
    var hash = r.IsDBNull(1) ? null : r.GetString(1);
    var orgId = r.IsDBNull(2) ? (Guid?)null : r.GetGuid(2);
    var pwOk = plainPassword is not null && hash is not null && BCrypt.Net.BCrypt.Verify(plainPassword, hash);
    Console.WriteLine(
        plainPassword is null
            ? $"email={r.GetString(0)} org={orgId} has_password_hash={hash is not null}"
            : $"email={r.GetString(0)} org={orgId} passwordOk={pwOk}");
}
