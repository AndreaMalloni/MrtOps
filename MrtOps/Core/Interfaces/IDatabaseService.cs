using System.Collections.Generic;
using System.Threading.Tasks;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

public interface IDatabaseService
{
    Task<List<DatabaseInfo>> GetAvailableDatabasesAsync(string serverAddress, bool useWindowsAuthentication, string? username = null, string? password = null);
}