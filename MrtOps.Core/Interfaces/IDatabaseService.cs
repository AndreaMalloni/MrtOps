using System.Collections.Generic;
using System.Threading.Tasks;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for the database management service.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Gets the list of available databases.
    /// </summary>
    /// <param name="serverAddress">The server address.</param>
    /// <param name="useWindowsAuthentication">Indicates whether to use Windows authentication.</param>
    /// <param name="username">The username (optional).</param>
    /// <param name="password">The password (optional).</param>
    /// <returns>A list of information about available databases.</returns>
    Task<List<DatabaseInfo>> GetAvailableDatabasesAsync(string serverAddress, bool useWindowsAuthentication, string? username = null, string? password = null);
}