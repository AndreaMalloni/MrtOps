using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using MrtOps.Core.Storage;

namespace MrtOps.Core;

/// <summary>
/// Represents a preview of an operation that can be executed or undone.
/// </summary>
/// <param name="Description">Description of the operation.</param>
/// <param name="TargetFilePath">Path of the target file.</param>
/// <param name="BackupFilePath">Path of the backup file, if any.</param>
/// <param name="IsCreation">Indicates if the operation is a creation.</param>
public record OperationPreview(
    string Description,
    string TargetFilePath,
    string? BackupFilePath,
    bool IsCreation
);

/// <summary>
/// Operation history manager, allows executing and undoing operations.
/// </summary>
public class OperationHistoryManager
{
    private readonly Stack<IOperation> _history = new();
    private readonly IHistoryStorage _storage;

    /// <summary>
    /// Initializes a new instance of <see cref="OperationHistoryManager"/>.
    /// </summary>
    /// <param name="storage">Optional storage for the history.</param>
    public OperationHistoryManager(IHistoryStorage? storage = null)
    {
        _storage = storage ?? new JsonHistoryStorage();
    }

    /// <summary>
    /// Executes an operation and adds it to the history.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <returns><c>true</c> if the operation was successful, <c>false</c> otherwise.</returns>
    public bool Execute(IOperation operation)
    {
        if (operation.Execute())
        {
            _history.Push(operation);

            if (operation is IReversibleOperation rev)
            {
                var entries = _storage.Load();
                entries.Add(new HistoryEntry(
                    rev.OperationType,
                    rev.Description,
                    rev.TargetFilePath,
                    rev.BackupFilePath,
                    DateTime.Now
                ));
                _storage.Save(entries);
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to preview the last performed operation.
    /// </summary>
    /// <param name="preview">The result of the operation preview.</param>
    /// <returns><c>true</c> if there is an operation in the history, <c>false</c> otherwise.</returns>
    public bool TryPeekLast(out OperationPreview? preview)
    {
        preview = null;

        if (_history.Count > 0)
        {
            var op = _history.Peek();
            if (op is IReversibleOperation rev)
            {
                preview = new OperationPreview(
                    rev.Description,
                    rev.TargetFilePath,
                    rev.BackupFilePath,
                    string.IsNullOrEmpty(rev.BackupFilePath)
                );
            }
            else
            {
                preview = new OperationPreview(op.Description, string.Empty, null, false);
            }
            return true;
        }

        var entries = _storage.Load();
        if (entries.Count > 0)
        {
            var last = entries.Last();
            preview = new OperationPreview(
                last.Description,
                last.TargetFilePath,
                last.BackupFilePath,
                string.IsNullOrEmpty(last.BackupFilePath)
            );
            return true;
        }

        return false;
    }

    /// <summary>
    /// Undoes the last performed operation.
    /// </summary>
    /// <param name="description">Description of the undone operation.</param>
    /// <returns><c>true</c> if the undo was successful, <c>false</c> otherwise.</returns>
    public bool UndoLast(out string description)
    {
        description = string.Empty;

        if (_history.Count > 0)
        {
            var operation = _history.Pop();
            description = operation.Description;
            bool success = operation.Undo();

            if (success)
            {
                var entries = _storage.Load();
                if (entries.Count > 0)
                {
                    entries.RemoveAt(entries.Count - 1);
                    _storage.Save(entries);
                }
            }

            return success;
        }

        var storedEntries = _storage.Load();
        if (storedEntries.Count == 0) return false;

        var lastEntry = storedEntries.Last();
        description = lastEntry.Description;

        bool rollbackSuccess = false;
        try
        {
            if (!string.IsNullOrEmpty(lastEntry.BackupFilePath) && File.Exists(lastEntry.BackupFilePath))
            {
                File.Copy(lastEntry.BackupFilePath, lastEntry.TargetFilePath, overwrite: true);
                File.Delete(lastEntry.BackupFilePath);
                rollbackSuccess = true;
            }
            else if (File.Exists(lastEntry.TargetFilePath))
            {
                File.Delete(lastEntry.TargetFilePath);
                rollbackSuccess = true;
            }
            else
            {
                // File non più presente, rollback considerato completato
                rollbackSuccess = true;
            }

            if (rollbackSuccess)
            {
                storedEntries.RemoveAt(storedEntries.Count - 1);
                _storage.Save(storedEntries);
            }
        }
        catch
        {
            rollbackSuccess = false;
        }

        return rollbackSuccess;
    }
}