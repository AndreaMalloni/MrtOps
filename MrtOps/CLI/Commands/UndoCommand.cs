using System;
using System.ComponentModel;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.CLI.Commands;

public class UndoSettings : CommandSettings
{
    [CommandOption("-y|--yes")]
    [Description("Conferma automaticamente il ripristino senza chiedere conferma")]
    public bool Yes { get; set; }
}

public class UndoCommand : Command<UndoSettings>
{
    private readonly OperationHistoryManager _history;
    private readonly ILocalizationService _loc;

    public UndoCommand(OperationHistoryManager history, ILocalizationService loc)
    {
        _history = history;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, UndoSettings settings, CancellationToken cancellationToken)
    {
        if (!_history.TryPeekLast(out var preview) || preview == null)
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoFail"));
            return 0;
        }

        AnsiConsole.MarkupLine(_loc.GetString("UndoPreviewHeader", preview.Description));

        if (preview.IsCreation || string.IsNullOrEmpty(preview.BackupFilePath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoActionDelete", preview.TargetFilePath));
        }
        else
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoActionRestore", preview.TargetFilePath, preview.BackupFilePath));
        }

        AnsiConsole.WriteLine();

        bool confirmed = settings.Yes;

        if (!confirmed)
        {
            if (Console.IsInputRedirected)
            {
                AnsiConsole.MarkupLine($"[bold]{_loc.GetString("PromptUndoConfirm")}[/] [grey](y/n)[/]:");
                string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
                confirmed = input == "y" || input == "yes" || input == "s" || input == "si" || input == "sì";
            }
            else
            {
                confirmed = AnsiConsole.Confirm(_loc.GetString("PromptUndoConfirm"), defaultValue: false);
            }
        }

        if (!confirmed)
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoCancelled"));
            return 0;
        }

        if (_history.UndoLast(out string description))
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoSuccess", description));
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoError"));
            return 1;
        }
    }
}