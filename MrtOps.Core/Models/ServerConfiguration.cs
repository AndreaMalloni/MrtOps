namespace MrtOps.Core.Models;

/// <summary>
/// Defines the supported database types for connections.
/// </summary>
public enum DatabaseType
{
    SqlServer,
    MySql,
    PostgreSql,
    MariaDb
}

/// <summary>
/// Represents the configuration for a target server.
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Gets or sets the alias for the server.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database type of the server.
    /// </summary>
    public DatabaseType Type { get; set; }

    /// <summary>
    /// Gets or sets the address or hostname of the server.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use Windows Authentication.
    /// </summary>
    public bool UseWindowsAuth { get; set; }

    /// <summary>
    /// Gets or sets the username for the connection.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for the connection.
    /// </summary>
    public string? Password { get; set; }
}
