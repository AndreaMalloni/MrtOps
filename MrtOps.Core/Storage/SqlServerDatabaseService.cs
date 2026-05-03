using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

public class SqlServerDatabaseService : IDatabaseService
{
    public async Task<List<DatabaseInfo>> GetAvailableDatabasesAsync(string serverAddress, bool useWindowsAuthentication, string? username = null, string? password = null)
    {
        var databases = new List<DatabaseInfo>();
        var baseConnectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = serverAddress,
            IntegratedSecurity = useWindowsAuthentication,
            TrustServerCertificate = true,
            ConnectTimeout = 5
        };

        if (!useWindowsAuthentication)
        {
            baseConnectionStringBuilder.UserID = username;
            baseConnectionStringBuilder.Password = password;
        }

        using var connection = new SqlConnection(baseConnectionStringBuilder.ConnectionString);
        await connection.OpenAsync();

        var query = "SELECT name FROM sys.databases WHERE database_id > 4 AND state = 0";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var dbName = reader.GetString(0);
            var dbBuilder = new SqlConnectionStringBuilder(baseConnectionStringBuilder.ConnectionString)
            {
                InitialCatalog = dbName
            };

            databases.Add(new DatabaseInfo(dbName, dbBuilder.ConnectionString, $"{serverAddress} - {dbName}"));
        }

        return databases;
    }
}