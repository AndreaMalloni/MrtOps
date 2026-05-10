using System.ComponentModel;
using Microsoft.Extensions.Logging;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using MrtOps.Core.Operations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

public class GenerateSettings : CommandSettings
{
    [CommandArgument(0, "[PATH]")]
    public string? Path { get; set; }

    [CommandOption("-n|--name")]
    public string? Name { get; set; }

    [CommandOption("-t|--template")]
    public string? Template { get; set; }
}

public class GenerateCommand : Command<GenerateSettings>
{
    private readonly IReportEngine _engine;
    private readonly ITemplateRepository _templates;
    private readonly OperationHistoryManager _history;
    private readonly ILocalizationService _loc;
    private readonly ILogger<CreateReportOperation> _logger;

    public GenerateCommand(IReportEngine engine, ITemplateRepository templates, OperationHistoryManager history, ILocalizationService loc, ILoggerFactory loggerFactory)
    {
        _engine = engine;
        _templates = templates;
        _logger = loggerFactory.CreateLogger<CreateReportOperation>();
        _history = history;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("MrtOps").Color(Color.Blue));

        var path = settings.Path ?? ".//";
        var name = settings.Name ?? "Report";
        var templateName = settings.Template ?? "";

        var metadata = new ReportMetadata(name, name, string.Empty, path, templateName);

        var operation = new CreateReportOperation(_engine, _loc, _templates, metadata, _logger);

        _history.Execute(operation);

        AnsiConsole.MarkupLine(_loc.GetString("SuccessReport", path));
        return 0;
    }
}