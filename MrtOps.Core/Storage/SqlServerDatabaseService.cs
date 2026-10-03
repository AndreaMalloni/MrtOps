using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

/// <summary>
/// Provides services for interacting with a SQL Server database.
/// </summary>
public class SqlServerDatabaseService : IDatabaseService
{
    /// <summary>
    /// Gets a list of available databases on the specified server.
    /// </summary>
    /// <param name="serverAddress">The SQL server address.</param>
    /// <param name="useWindowsAuthentication">Indicates whether to use Windows authentication.</param>
    /// <param name="username">The optional username for SQL authentication.</param>
    /// <param name="password">The optional password for SQL authentication.</param>
    /// <returns>A list of information about available databases.</returns>
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