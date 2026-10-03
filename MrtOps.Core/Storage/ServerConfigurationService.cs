using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

/// <summary>
/// Implementation of IServerConfigurationService that encrypts and stores configurations locally.
/// </summary>
[SupportedOSPlatform("windows")]
public class ServerConfigurationService : IServerConfigurationService
{
    private readonly string _configFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConfigurationService"/> class.
    /// </summary>
    public ServerConfigurationService()
    {
        var appFolder = AppDomain.CurrentDomain.BaseDirectory;
        _configFilePath = Path.Combine(appFolder, "servers.dat");
    }

    /// <inheritdoc />
    public async Task<List<ServerConfiguration>> GetServersAsync()
    {
        if (!File.Exists(_configFilePath))
        {
            return new List<ServerConfiguration>();
        }

        var encryptedBytes = await File.ReadAllBytesAsync(_configFilePath);
        try
        {
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decryptedBytes);
            return JsonSerializer.Deserialize<List<ServerConfiguration>>(json) ?? new List<ServerConfiguration>();
        }
        catch (CryptographicException)
        {
            return new List<ServerConfiguration>();
        }
    }

    /// <inheritdoc />
    public async Task AddServerAsync(ServerConfiguration server)
    {
        var servers = await GetServersAsync();
        
        var existing = servers.FirstOrDefault(s => s.Alias.Equals(server.Alias, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            throw new InvalidOperationException($"Server with alias '{server.Alias}' already exists.");
        }
        
        servers.Add(server);
        await SaveAsync(servers);
    }

    /// <inheritdoc />
    public async Task UpdateServerAsync(ServerConfiguration server)
    {
        var servers = await GetServersAsync();
        
        var existing = servers.FirstOrDefault(s => s.Alias.Equals(server.Alias, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            throw new InvalidOperationException($"Server with alias '{server.Alias}' not found.");
        }
        
        servers.Remove(existing);
        servers.Add(server);
        await SaveAsync(servers);
    }

    /// <inheritdoc />
    public async Task RemoveServerAsync(string alias)
    {
        var servers = await GetServersAsync();
        var server = servers.FirstOrDefault(s => s.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
        if (server != null)
        {
            servers.Remove(server);
            await SaveAsync(servers);
        }
    }

    private async Task SaveAsync(List<ServerConfiguration> servers)
    {
        var json = JsonSerializer.Serialize(servers);
        var bytes = Encoding.UTF8.GetBytes(json);
        var encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_configFilePath, encryptedBytes);
    }
}
