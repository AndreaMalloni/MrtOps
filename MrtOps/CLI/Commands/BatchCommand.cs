using System.ComponentModel;
using MrtOps.Application.Services;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.Presentation.CLI.Commands;

public class BatchSettings : CommandSettings
{
    [CommandArgument(0, "[FOLDER_PATH]")]
    public string? FolderPath { get; set; }

    [CommandOption("--add-var")]
    public string? VarName { get; set; }

    [CommandOption("--category")]
    [DefaultValue("ParameterFields")]
    public string? Category { get; set; }

    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }
}

public class BatchCommand : Command<BatchSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public BatchCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, BatchSettings settings, CancellationToken cancellationToken)
    {
        var folder = settings.FolderPath ?? AnsiConsole.Ask<string>(_loc.GetString("PromptFolder"));
        var varName = settings.VarName ?? AnsiConsole.Ask<string>(_loc.GetString("PromptVar"));
        var category = settings.Category ?? "ParameterFields";

        _batchService.ProcessFolderAddVariable(folder, category, varName, settings.DryRun);
        return 0;
    }
}