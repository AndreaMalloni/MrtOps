using System.ComponentModel;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

public class BatchSettingsBase : CommandSettings
{
    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }
}

public class BatchVarSettings : BatchSettingsBase
{
    [CommandArgument(0, "[TARGET_PATHS]")]
    public string[]? TargetPaths { get; set; }

    [CommandOption("--action|-a")]
    [DefaultValue("add")]
    public string Action { get; set; } = "add"; // add, remove, modify

    [CommandOption("--var|-v")]
    public string? VarName { get; set; }

    [CommandOption("--new-name|-n")]
    public string? NewName { get; set; }

    [CommandOption("--category|-c")]
    [DefaultValue("ParameterFields")]
    public string Category { get; set; } = "ParameterFields";

    [CommandOption("--type|-t")]
    public string? Type { get; set; }

    [CommandOption("--init|-i")]
    public string? InitMethod { get; set; }
}

public class BatchVarCommand : Command<BatchVarSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public BatchVarCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, BatchVarSettings settings, CancellationToken cancellationToken)
    {
        var targetPaths = settings.TargetPaths != null && settings.TargetPaths.Length > 0 
            ? settings.TargetPaths 
            : new[] { AnsiConsole.Ask<string>(_loc.GetString("PromptFolder")) };
        var varName = settings.VarName ?? AnsiConsole.Ask<string>(_loc.GetString("PromptVar"));

        _batchService.ProcessVariableOperation(targetPaths, settings.Action, settings.Category, varName, settings.NewName, settings.Type, settings.InitMethod, settings.DryRun);
        return 0;
    }
}

public class BatchDsSettings : BatchSettingsBase
{
    [CommandArgument(0, "[TARGET_PATHS]")]
    public string[]? TargetPaths { get; set; }

    [CommandOption("--action|-a")]
    [DefaultValue("update-conn")]
    public string Action { get; set; } = "update-conn";

    [CommandOption("--db|-d")]
    public string? DatabaseName { get; set; }

    [CommandOption("--conn|-c")]
    public string? ConnectionString { get; set; }
}

public class BatchDsCommand : Command<BatchDsSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public BatchDsCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, BatchDsSettings settings, CancellationToken cancellationToken)
    {
        var targetPaths = settings.TargetPaths != null && settings.TargetPaths.Length > 0 ? settings.TargetPaths : new[] { AnsiConsole.Ask<string>(_loc.GetString("PromptFolder")) };
        var dbName = settings.DatabaseName ?? AnsiConsole.Ask<string>("Database Name:");
        var connString = settings.ConnectionString ?? AnsiConsole.Ask<string>("New Connection String:");

        _batchService.ProcessDataSourceOperation(targetPaths, settings.Action, dbName, connString, settings.DryRun);
        return 0;
    }
}

public class BatchPropSettings : BatchSettingsBase
{
    [CommandArgument(0, "[TARGET_PATHS]")]
    public string[]? TargetPaths { get; set; }

    [CommandOption("--action|-a")]
    [DefaultValue("set")]
    public string Action { get; set; } = "set";

    [CommandOption("--prop|-p")]
    public string? PropertyName { get; set; }

    [CommandOption("--val|-v")]
    public string? PropertyValue { get; set; }
}

public class BatchPropCommand : Command<BatchPropSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public BatchPropCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, BatchPropSettings settings, CancellationToken cancellationToken)
    {
        var targetPaths = settings.TargetPaths != null && settings.TargetPaths.Length > 0 ? settings.TargetPaths : new[] { AnsiConsole.Ask<string>(_loc.GetString("PromptFolder")) };
        var propName = settings.PropertyName ?? AnsiConsole.Ask<string>("Property Name (e.g. Author, ConvertNulls):");
        var propVal = settings.PropertyValue ?? AnsiConsole.Ask<string>("New Value:");

        _batchService.ProcessPropertyOperation(targetPaths, settings.Action, propName, propVal, settings.DryRun);
        return 0;
    }
}

public class BatchCleanSettings : BatchSettingsBase
{
    [CommandArgument(0, "[TARGET_PATHS]")]
    public string[]? TargetPaths { get; set; }

    [CommandOption("--action|-a")]
    [DefaultValue("vars")]
    public string Action { get; set; } = "vars"; // vars, sources
}

public class BatchCleanCommand : Command<BatchCleanSettings>
{
    private readonly BatchProcessingService _batchService;
    private readonly ILocalizationService _loc;

    public BatchCleanCommand(BatchProcessingService batchService, ILocalizationService loc)
    {
        _batchService = batchService;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, BatchCleanSettings settings, CancellationToken cancellationToken)
    {
        var targetPaths = settings.TargetPaths != null && settings.TargetPaths.Length > 0 ? settings.TargetPaths : new[] { AnsiConsole.Ask<string>(_loc.GetString("PromptFolder")) };
        _batchService.ProcessCleanOperation(targetPaths, settings.Action, settings.DryRun);
        return 0;
    }
}