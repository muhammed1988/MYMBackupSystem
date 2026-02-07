using ClientBackupAgent.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ClientBackupAgent.Services;

public class SqlBackupService
{
    private readonly SqlOptions _options;

    public SqlBackupService(IOptions<SqlOptions> options)
    {
        _options = options.Value;
    }

    private string BuildConnectionString(string? database = "master")
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = _options.Server,
            InitialCatalog = database ?? "master",
            IntegratedSecurity = _options.UseIntegratedSecurity
        };

        if (!_options.UseIntegratedSecurity)
        {
            builder.UserID = _options.Username;
            builder.Password = _options.Password;
        }

        return builder.ConnectionString;
    }

    public async Task<List<string>> GetUserDatabasesAsync(CancellationToken ct)
    {
        const string sql = @"
            SELECT name 
            FROM sys.databases 
            WHERE database_id > 4 AND state = 0;";

        var result = new List<string>();

        await using var conn = new SqlConnection(BuildConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public async Task<string> BackupDatabaseAsync(string dbName, string tempPath, CancellationToken ct)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var bakFile = Path.Combine(tempPath, $"{dbName}_{timestamp}.bak");

        var sql = $@"
            BACKUP DATABASE [{dbName}]
            TO DISK = N'{bakFile.Replace("'", "''")}'
            WITH INIT;"; // , COMPRESSION;

        await using var conn = new SqlConnection(BuildConnectionString("master"));
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandTimeout = 60 * 60
        };

        await cmd.ExecuteNonQueryAsync(ct);

        return bakFile;
    }
}
