using System.ComponentModel;
using MrtOps.Application.Services;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.Presentation.CLI.Commands;

public class SyncStyleSettings : CommandSettings
{
    [CommandArgument(0, "[FOLDER_PATH]")]
    public string? FolderPath { get; set; }

    [CommandArgument(1, "[STYLE_FILE]")]
    public string? StyleFilePath { get; set; }

    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }
}

public class SyncStyleCommand : Command<SyncStyleSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public SyncStyleCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, SyncStyleSettings settings, CancellationToken cancellationToken)
    {
        var folder = settings.FolderPath ?? AnsiConsole.Ask<string>(_loc.GetString("PromptFolder"));
        var styleFile = settings.StyleFilePath ?? AnsiConsole.Ask<string>(_loc.GetString("PromptStyle"));

        _batchService.ProcessFolderApplyStyle(folder, styleFile, settings.DryRun);
        return 0;
    }
}