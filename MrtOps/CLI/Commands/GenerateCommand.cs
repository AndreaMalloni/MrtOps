using System.ComponentModel;
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

    public GenerateCommand(IReportEngine engine, ITemplateRepository templates, OperationHistoryManager history, ILocalizationService loc)
    {
        _engine = engine;
        _templates = templates;
        _history = history;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("MrtOps").Color(Color.Blue));

        var path = settings.Path ?? AnsiConsole.Ask<string>(_loc.GetString("PromptPath"));
        var name = settings.Name ?? AnsiConsole.Ask<string>(_loc.GetString("PromptName"));
        var templateName = settings.Template ?? AnsiConsole.Ask<string>(_loc.GetString("PromptTemplate"), "Standard_Template");

        var template = _templates.GetTemplate(templateName);
        if (template == null)
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorTemplateNotFound", templateName));
            return -1;
        }

        var metadata = new ReportMetadata(name, name, string.Empty, path, templateName);
        var operation = new CreateReportOperation(_engine, _loc, metadata, template);

        _history.Execute(operation);

        AnsiConsole.MarkupLine(_loc.GetString("SuccessReport", path));
        return 0;
    }
}