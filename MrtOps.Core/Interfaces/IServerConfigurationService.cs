using System.Collections.Generic;
using System.Threading.Tasks;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Provides methods to manage server configurations.
/// </summary>
public interface IServerConfigurationService
{
    /// <summary>
    /// Retrieves the list of configured servers.
    /// </summary>
    /// <returns>A list of server configurations.</returns>
    Task<List<ServerConfiguration>> GetServersAsync();

    /// <summary>
    /// Adds a new server configuration.
    /// </summary>
    /// <param name="server">The server configuration to add.</param>
    Task AddServerAsync(ServerConfiguration server);

    /// <summary>
    /// Updates an existing server configuration.
    /// </summary>
    /// <param name="server">The server configuration to update.</param>
    Task UpdateServerAsync(ServerConfiguration server);

    /// <summary>
    /// Removes a server configuration by its alias.
    /// </summary>
    /// <param name="alias">The alias of the server to remove.</param>
    Task RemoveServerAsync(string alias);
}
