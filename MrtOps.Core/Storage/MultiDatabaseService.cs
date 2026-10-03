using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

/// <summary>
/// Implementation of IDatabaseService that supports multiple database types.
/// </summary>
public class MultiDatabaseService : IDatabaseService
{
    /// <inheritdoc />
    public async Task<List<DatabaseInfo>> GetAvailableDatabasesAsync(ServerConfiguration serverConfig)
    {
        return serverConfig.Type switch
        {
            DatabaseType.SqlServer => await GetSqlServerDatabasesAsync(serverConfig),
            DatabaseType.MySql => await GetMySqlDatabasesAsync(serverConfig),
            DatabaseType.MariaDb => await GetMySqlDatabasesAsync(serverConfig),
            DatabaseType.PostgreSql => await GetPostgreSqlDatabasesAsync(serverConfig),
            _ => throw new NotSupportedException($"Database type {serverConfig.Type} is not supported.")
        };
    }

    private async Task<List<DatabaseInfo>> GetSqlServerDatabasesAsync(ServerConfiguration config)
    {
        var databases = new List<DatabaseInfo>();
        var baseConnectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = config.Address,
            IntegratedSecurity = config.UseWindowsAuth,
            TrustServerCertificate = true,
            ConnectTimeout = 5
        };

        if (!config.UseWindowsAuth)
        {
            baseConnectionStringBuilder.UserID = config.Username ?? "";
            baseConnectionStringBuilder.Password = config.Password ?? "";
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

            databases.Add(new DatabaseInfo(dbName, dbBuilder.ConnectionString, $"{config.Alias} - {dbName}"));
        }

        return databases;
    }

    private async Task<List<DatabaseInfo>> GetMySqlDatabasesAsync(ServerConfiguration config)
    {
        var databases = new List<DatabaseInfo>();
        var baseConnectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = config.Address,
            ConnectionTimeout = 5
        };

        if (!config.UseWindowsAuth)
        {
            baseConnectionStringBuilder.UserID = config.Username ?? "";
            baseConnectionStringBuilder.Password = config.Password ?? "";
        }

        using var connection = new MySqlConnection(baseConnectionStringBuilder.ConnectionString);
        await connection.OpenAsync();

        var query = "SHOW DATABASES;";
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var dbName = reader.GetString(0);
            
            if (dbName == "information_schema" || dbName == "mysql" || dbName == "performance_schema" || dbName == "sys")
                continue;

            var dbBuilder = new MySqlConnectionStringBuilder(baseConnectionStringBuilder.ConnectionString)
            {
                Database = dbName
            };

            databases.Add(new DatabaseInfo(dbName, dbBuilder.ConnectionString, $"{config.Alias} - {dbName}"));
        }

        return databases;
    }

    private async Task<List<DatabaseInfo>> GetPostgreSqlDatabasesAsync(ServerConfiguration config)
    {
        var databases = new List<DatabaseInfo>();
        
        var baseConnectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = config.Address,
            Database = "postgres", 
            Timeout = 5
        };

        if (!config.UseWindowsAuth)
        {
            baseConnectionStringBuilder.Username = config.Username ?? "";
            baseConnectionStringBuilder.Password = config.Password ?? "";
        }

        using var connection = new NpgsqlConnection(baseConnectionStringBuilder.ConnectionString);
        await connection.OpenAsync();

        var query = "SELECT datname FROM pg_database WHERE datistemplate = false;";
        using var command = new NpgsqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var dbName = reader.GetString(0);
            
            var dbBuilder = new NpgsqlConnectionStringBuilder(baseConnectionStringBuilder.ConnectionString)
            {
                Database = dbName
            };

            databases.Add(new DatabaseInfo(dbName, dbBuilder.ConnectionString, $"{config.Alias} - {dbName}"));
        }

        return databases;
    }
}