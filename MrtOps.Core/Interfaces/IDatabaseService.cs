using System.Collections.Generic;
using System.Threading.Tasks;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Provides methods to interact with various database management systems.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Connects to a server and retrieves a list of available databases.
    /// </summary>
    /// <param name="serverConfig">The server configuration.</param>
    /// <returns>A list of database information objects.</returns>
    Task<List<DatabaseInfo>> GetAvailableDatabasesAsync(ServerConfiguration serverConfig);
}