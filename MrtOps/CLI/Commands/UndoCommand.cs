using MrtOps.Core.History;
using MrtOps.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrtOps.Presentation.CLI.Commands;

public class UndoCommand : Command<EmptyCommandSettings>
{
    private readonly OperationHistoryManager _history;
    private readonly ILocalizationService _loc;

    public UndoCommand(OperationHistoryManager history, ILocalizationService loc)
    {
        _history = history;
        _loc = loc;
    }

    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        if (_history.UndoLast(out string description))
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoSuccess", description));
        }
        else
        {
            AnsiConsole.MarkupLine(_loc.GetString("UndoFail"));
        }
        return 0;
    }
}