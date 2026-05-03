using System.Collections.Generic;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core;

public class OperationHistoryManager
{
    private readonly Stack<IOperation> _history = new();

    public bool Execute(IOperation operation)
    {
        if (operation.Execute())
        {
            _history.Push(operation);
            return true;
        }
        return false;
    }

    public bool UndoLast(out string description)
    {
        description = string.Empty;
        if (_history.Count == 0) return false;

        var operation = _history.Pop();
        description = operation.Description;
        return operation.Undo();
    }
}