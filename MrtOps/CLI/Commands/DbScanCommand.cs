using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

/// <summary>
/// Settings for the DbScanCommand.
/// </summary>
public class DbScanSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the server alias.
    /// </summary>
    [CommandOption("-s|--server")]
    public string? ServerAlias { get; set; }
}

/// <summary>
/// Command to scan a database server and generate connection strings.
/// </summary>
public class DbScanCommand : AsyncCommand<DbScanSettings>
{
    private readonly IDatabaseService _dbService;
    private readonly IServerConfigurationService _configService;
    private readonly ILocalizationService _loc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanCommand"/> class.
    /// </summary>
    public DbScanCommand(IDatabaseService dbService, IServerConfigurationService configService, ILocalizationService loc)
    {
        _dbService = dbService;
        _configService = configService;
        _loc = loc;
    }

    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, DbScanSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("MrtOps DB").Color(Color.Green));

        var servers = await _configService.GetServersAsync();
        if (servers.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No servers configured. Use 'server add' to add a server first.[/]");
            return 1;
        }

        ServerConfiguration? selectedServer = null;

        if (!string.IsNullOrEmpty(settings.ServerAlias))
        {
            selectedServer = servers.FirstOrDefault(s => s.Alias.Equals(settings.ServerAlias, StringComparison.OrdinalIgnoreCase));
            if (selectedServer == null)
            {
                var available = string.Join(", ", servers.Select(s => s.Alias));
                AnsiConsole.MarkupLine($"[red]Server '{settings.ServerAlias}' not found. Available servers are:[/] {available}");
                return 1;
            }
        }
        else
        {
            var prompt = new SelectionPrompt<ServerConfiguration>()
                .Title("Select a server to scan:")
                .PageSize(10)
                .UseConverter(s => $"{s.Alias} ({s.Type} @ {s.Address})")
                .AddChoices(servers);
            
            selectedServer = AnsiConsole.Prompt(prompt);
        }

        AnsiConsole.MarkupLine($"Connecting to server: [bold]{selectedServer.Alias}[/]...");

        if (!selectedServer.UseWindowsAuth && string.IsNullOrEmpty(selectedServer.Password))
        {
            var p = new TextPrompt<string>($"Password for [bold]{selectedServer.Username}[/] on [bold]{selectedServer.Alias}[/]:")
                .PromptStyle("red")
                .Secret()
                .AllowEmpty();
            
            var pass = AnsiConsole.Prompt(p);
            
            selectedServer.Password = pass;
            await _configService.UpdateServerAsync(selectedServer);
            AnsiConsole.MarkupLine("[green]Password saved to configuration.[/]");
        }

        var databases = await AnsiConsole.Status().StartAsync("Scanning Databases...", async ctx =>
        {
            try
            {
                return await _dbService.GetAvailableDatabasesAsync(selectedServer);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to connect:[/] {ex.Message}");
                return null;
            }
        });

        if (databases == null) return 1;

        if (databases.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No databases found on this server.[/]");
            return 0;
        }

        var dbPrompt = new SelectionPrompt<DatabaseInfo>()
            .Title("Select a database:")
            .PageSize(10)
            .UseConverter(d => d.Name)
            .AddChoices(databases);

        var selectedDb = AnsiConsole.Prompt(dbPrompt);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]Connection String for Stimulsoft:[/]");
        var panel = new Panel(selectedDb.ConnectionString)
        {
            Header = new PanelHeader($"{selectedServer.Type} - {selectedDb.Name}"),
            Border = BoxBorder.Rounded,
            Expand = true
        };
        AnsiConsole.Write(panel);

        return 0;
    }
}