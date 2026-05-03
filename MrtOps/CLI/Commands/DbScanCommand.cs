using System.Threading.Tasks;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

public class DbScanSettings : CommandSettings
{
    [CommandOption("-s|--server")]
    public string? Server { get; set; }

    [CommandOption("--sql-auth")]
    public bool SqlAuth { get; set; }
}

public class DbScanCommand : AsyncCommand<DbScanSettings>
{
    private readonly IDatabaseService _dbService;
    private readonly ILocalizationService _loc;

    public DbScanCommand(IDatabaseService dbService, ILocalizationService loc)
    {
        _dbService = dbService;
        _loc = loc;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, DbScanSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("MrtOps DB").Color(Color.Green));

        var server = settings.Server ?? AnsiConsole.Ask<string>(_loc.GetString("PromptServer"));
        var useWindowsAuth = !settings.SqlAuth;
        string? user = null;
        string? pass = null;

        if (!useWindowsAuth)
        {
            user = AnsiConsole.Ask<string>(_loc.GetString("PromptUser"));
            pass = AnsiConsole.Ask<string>(_loc.GetString("PromptPass"));
        }

        await AnsiConsole.Status().StartAsync(_loc.GetString("ScanningDatabases"), async ctx =>
        {
            var databases = await _dbService.GetAvailableDatabasesAsync(server, useWindowsAuth, user, pass);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(_loc.GetString("DbListHeader", server));

            foreach (var db in databases)
            {
                var panel = new Panel(db.ConnectionString)
                {
                    Header = new PanelHeader(db.Name),
                    Border = BoxBorder.Rounded
                };
                AnsiConsole.Write(panel);
            }
        });

        return 0;
    }
}