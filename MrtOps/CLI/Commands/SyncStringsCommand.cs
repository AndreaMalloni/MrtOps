using MrtOps.Core;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

public class SyncStringsSettings : CommandSettings
{
    [CommandArgument(0, "[FOLDER_PATH]")]
    public string? FolderPath { get; set; }

    [CommandArgument(1, "[STRINGS_FILE]")]
    public string? StringsFilePath { get; set; }

    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }
}

public class SyncStringsCommand : Command<SyncStringsSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public SyncStringsCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, SyncStringsSettings settings, CancellationToken cancellationToken)
    {
        var folder = settings.FolderPath ?? AnsiConsole.Ask<string>(_loc.GetString("PromptFolder"));
        var stringsFile = settings.StringsFilePath ?? AnsiConsole.Ask<string>(_loc.GetString("PromptStringsFile"));

        _batchService.ProcessFolderSyncStrings(folder, stringsFile, settings.DryRun);
        return 0;
    }
}