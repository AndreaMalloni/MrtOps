using System.ComponentModel;
using System.Threading.Tasks;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

/// <summary>
/// Settings for adding a new server.
/// </summary>
public class ServerAddSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the alias for the server.
    /// </summary>
    [CommandOption("-a|--alias <ALIAS>")]
    [Description("Alias for the server")]
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the database type.
    /// </summary>
    [CommandOption("-t|--type <TYPE>")]
    [Description("Database type (SqlServer, MySql, PostgreSql, MariaDb)")]
    public string? TypeName { get; set; }

    /// <summary>
    /// Gets the parsed database type.
    /// </summary>
    public DatabaseType? ParsedType
    {
        get
        {
            if (string.IsNullOrEmpty(TypeName)) return null;
            if (System.Enum.TryParse<DatabaseType>(TypeName, true, out var result)) return result;
            return null;
        }
    }

    /// <inheritdoc />
    public override ValidationResult Validate()
    {
        if (!string.IsNullOrEmpty(TypeName) && ParsedType == null)
        {
            var options = string.Join(", ", System.Enum.GetNames(typeof(DatabaseType)));
            return ValidationResult.Error($"Type '{TypeName}' is not supported. Supported options are: {options}");
        }
        return ValidationResult.Success();
    }

    /// <summary>
    /// Gets or sets the server address or host.
    /// </summary>
    [CommandOption("-h|--host <HOST>")]
    [Description("Server address or host")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use Windows Authentication.
    /// </summary>
    [CommandOption("-w|--windows-auth")]
    [Description("Use Windows Authentication (if applicable)")]
    public bool UseWindowsAuth { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [CommandOption("-u|--user <USER>")]
    [Description("Username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [CommandOption("-p|--password <PASSWORD>")]
    [Description("Password")]
    public string? Password { get; set; }
}

/// <summary>
/// Command to add a new server configuration.
/// </summary>
public class ServerAddCommand : AsyncCommand<ServerAddSettings>
{
    private readonly IServerConfigurationService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerAddCommand"/> class.
    /// </summary>
    public ServerAddCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ServerAddSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        var alias = settings.Alias ?? AnsiConsole.Ask<string>("Server Alias:");
        var type = settings.ParsedType ?? AnsiConsole.Prompt(
            new SelectionPrompt<DatabaseType>()
                .Title("Select Database Type:")
                .AddChoices(DatabaseType.SqlServer, DatabaseType.MySql, DatabaseType.MariaDb, DatabaseType.PostgreSql));
        
        var address = settings.Address ?? AnsiConsole.Ask<string>("Server Address/Host:");
        
        bool winAuth = settings.UseWindowsAuth;
        if (!settings.UseWindowsAuth && type == DatabaseType.SqlServer && string.IsNullOrEmpty(settings.Username) && string.IsNullOrEmpty(settings.Password))
        {
            winAuth = AnsiConsole.Confirm("Use Windows Authentication?");
        }

        string? user = settings.Username;
        string? pass = settings.Password;

        if (!winAuth && type != DatabaseType.SqlServer && string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
        {
            user ??= AnsiConsole.Ask<string>("Username:");
            pass ??= AnsiConsole.Prompt(
                new TextPrompt<string>("Password:")
                    .PromptStyle("red")
                    .Secret()
                    .AllowEmpty());
        }
        else if (!winAuth && type == DatabaseType.SqlServer && string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
        {
             user ??= AnsiConsole.Ask<string>("Username:");
             pass ??= AnsiConsole.Prompt(
                new TextPrompt<string>("Password:")
                    .PromptStyle("red")
                    .Secret()
                    .AllowEmpty());
        }
        else if (!winAuth && string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        {
            user = AnsiConsole.Ask<string>("Username:");
        }
        else if (!winAuth && !string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
        {
             pass = AnsiConsole.Prompt(
                new TextPrompt<string>("Password:")
                    .PromptStyle("red")
                    .Secret()
                    .AllowEmpty());
        }

        var serverConfig = new ServerConfiguration
        {
            Alias = alias,
            Type = type,
            Address = address,
            UseWindowsAuth = winAuth,
            Username = user,
            Password = pass
        };

        await _configService.AddServerAsync(serverConfig);
        AnsiConsole.MarkupLine($"[green]Server '{alias}' added successfully.[/]");
        return 0;
    }
}

public class ServerListCommand : AsyncCommand<EmptyCommandSettings>
{
    private readonly IServerConfigurationService _configService;

    public ServerListCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, EmptyCommandSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        var servers = await _configService.GetServersAsync();
        
        if (servers.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No servers configured.[/]");
            return 0;
        }

        var table = new Table().AddColumn("Alias").AddColumn("Type").AddColumn("Address").AddColumn("Auth");
        foreach (var s in servers)
        {
            table.AddRow(s.Alias, s.Type.ToString(), s.Address, s.UseWindowsAuth ? "Windows Auth" : "User/Pass");
        }

        AnsiConsole.Write(table);
        return 0;
    }
}

public class ServerRemoveSettings : CommandSettings
{
    [CommandArgument(0, "[ALIAS]")]
    [Description("Alias of the server to remove")]
    public string? Alias { get; set; }
}

public class ServerRemoveCommand : AsyncCommand<ServerRemoveSettings>
{
    private readonly IServerConfigurationService _configService;

    public ServerRemoveCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ServerRemoveSettings settings, CancellationToken cancellationToken)
    {
        var servers = await _configService.GetServersAsync();
        if (servers.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No servers configured.[/]");
            return 0;
        }

        string alias = settings.Alias ?? AnsiConsole.Ask<string>("Server Alias to remove:");
        
        var existing = System.Linq.Enumerable.FirstOrDefault(servers, s => s.Alias.Equals(alias, System.StringComparison.OrdinalIgnoreCase));
        if (existing == null && !string.IsNullOrEmpty(settings.Alias))
        {
            var available = string.Join(", ", System.Linq.Enumerable.Select(servers, s => s.Alias));
            AnsiConsole.MarkupLine($"[red]Server '{alias}' not found. Available servers are:[/] {available}");
            return 1;
        }
        else if (existing == null)
        {
            AnsiConsole.MarkupLine($"[red]Server '{alias}' not found.[/]");
            return 1;
        }

        await _configService.RemoveServerAsync(existing.Alias);
        AnsiConsole.MarkupLine($"[green]Server '{existing.Alias}' removed.[/]");
        return 0;
    }
}

public class ServerUpdateCommand : AsyncCommand<ServerAddSettings>
{
    private readonly IServerConfigurationService _configService;

    public ServerUpdateCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ServerAddSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        var alias = settings.Alias ?? AnsiConsole.Ask<string>("Server Alias to update:");
        
        var servers = await _configService.GetServersAsync();
        var existing = System.Linq.Enumerable.FirstOrDefault(servers, s => s.Alias.Equals(alias, System.StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            if (!string.IsNullOrEmpty(settings.Alias))
            {
                var available = string.Join(", ", System.Linq.Enumerable.Select(servers, s => s.Alias));
                AnsiConsole.MarkupLine($"[red]Server '{alias}' not found. Available servers are:[/] {available}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Server '{alias}' not found.[/]");
            }
            return 1;
        }

        var type = settings.ParsedType ?? existing.Type;
        if (settings.TypeName == null)
        {
            if (AnsiConsole.Confirm("Do you want to change the database type?", false))
            {
                type = AnsiConsole.Prompt(
                    new SelectionPrompt<DatabaseType>()
                        .Title("Select Database Type:")
                        .AddChoices(DatabaseType.SqlServer, DatabaseType.MySql, DatabaseType.MariaDb, DatabaseType.PostgreSql));
            }
        }
        
        var address = settings.Address ?? existing.Address;
        if (settings.Address == null)
        {
            address = AnsiConsole.Prompt(new TextPrompt<string>("Server Address/Host:").DefaultValue(existing.Address));
        }
        
        bool winAuth = settings.UseWindowsAuth;
        bool winAuthSpecified = context.Remaining.Parsed.Contains("windows-auth") || context.Remaining.Parsed.Contains("w");
        
        if (!winAuthSpecified)
        {
            winAuth = existing.UseWindowsAuth;
            if (type == DatabaseType.SqlServer && string.IsNullOrEmpty(settings.Username) && string.IsNullOrEmpty(settings.Password))
            {
                winAuth = AnsiConsole.Confirm("Use Windows Authentication?", existing.UseWindowsAuth);
            }
        }

        string? user = settings.Username ?? existing.Username;
        string? pass = settings.Password ?? existing.Password;

        if (!winAuth && settings.Username == null && settings.Password == null)
        {
            user = AnsiConsole.Prompt(new TextPrompt<string>("Username:").DefaultValue(existing.Username ?? ""));
            
            var passPrompt = new TextPrompt<string>("Password:").PromptStyle("red").Secret();
            passPrompt.AllowEmpty = true;
            var newPass = AnsiConsole.Prompt(passPrompt);
            if (!string.IsNullOrEmpty(newPass))
            {
                pass = newPass;
            }
        }

        var serverConfig = new ServerConfiguration
        {
            Alias = existing.Alias,
            Type = type,
            Address = address,
            UseWindowsAuth = winAuth,
            Username = user,
            Password = pass
        };

        await _configService.UpdateServerAsync(serverConfig);
        AnsiConsole.MarkupLine($"[green]Server '{existing.Alias}' updated successfully.[/]");
        return 0;
    }
}

public class ServerExportSettings : CommandSettings
{
    [CommandArgument(0, "<FILE_PATH>")]
    [Description("Path where to export the server configurations (JSON)")]
    public string? FilePath { get; set; }
}

public class ServerExportCommand : AsyncCommand<ServerExportSettings>
{
    private readonly IServerConfigurationService _configService;

    public ServerExportCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ServerExportSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        var servers = await _configService.GetServersAsync();
        
        if (servers.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No servers to export.[/]");
            return 0;
        }

        var exportList = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(servers, s => new ServerConfiguration
        {
            Alias = s.Alias,
            Type = s.Type,
            Address = s.Address,
            UseWindowsAuth = s.UseWindowsAuth,
            Username = s.Username,
            Password = null
        }));

        var json = System.Text.Json.JsonSerializer.Serialize(exportList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        
        var path = settings.FilePath ?? "servers_export.json";
        await System.IO.File.WriteAllTextAsync(path, json, cancellationToken);

        AnsiConsole.MarkupLine($"[green]Successfully exported {servers.Count} servers to '{path}'. Passwords were excluded.[/]");
        return 0;
    }
}

public class ServerImportSettings : CommandSettings
{
    [CommandArgument(0, "<FILE_PATH>")]
    [Description("Path of the JSON file to import")]
    public string? FilePath { get; set; }
}

public class ServerImportCommand : AsyncCommand<ServerImportSettings>
{
    private readonly IServerConfigurationService _configService;

    public ServerImportCommand(IServerConfigurationService configService)
    {
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ServerImportSettings settings, System.Threading.CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.FilePath) || !System.IO.File.Exists(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]File '{settings.FilePath}' not found.[/]");
            return 1;
        }

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(settings.FilePath, cancellationToken);
            var importedServers = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<ServerConfiguration>>(json);

            if (importedServers == null || importedServers.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No servers found in the file.[/]");
                return 0;
            }

            var currentServers = await _configService.GetServersAsync();
            int count = 0;

            foreach (var server in importedServers)
            {
                var existing = System.Linq.Enumerable.FirstOrDefault(currentServers, s => s.Alias.Equals(server.Alias, System.StringComparison.OrdinalIgnoreCase));
                
                if (existing != null)
                {
                    server.Password = server.Password ?? existing.Password;
                    await _configService.UpdateServerAsync(server);
                }
                else
                {
                    await _configService.AddServerAsync(server);
                }
                count++;
            }

            AnsiConsole.MarkupLine($"[green]Successfully imported {count} servers from '{settings.FilePath}'.[/]");
            return 0;
        }
        catch (System.Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to import servers:[/] {ex.Message}");
            return 1;
        }
    }
}
