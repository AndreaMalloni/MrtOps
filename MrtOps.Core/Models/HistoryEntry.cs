using System;

namespace MrtOps.Core.Models;

/// <summary>
/// Represents an entry in the history of performed operations.
/// </summary>
/// <param name="OperationType">The type of operation performed.</param>
/// <param name="Description">The description of the operation.</param>
/// <param name="TargetFilePath">The path of the modified target file.</param>
/// <param name="BackupFilePath">The optional path of the backup file.</param>
/// <param name="Timestamp">The date and time when the operation was recorded.</param>
public record HistoryEntry(
    string OperationType,
    string Description,
    string TargetFilePath,
    string? BackupFilePath,
    DateTime Timestamp
);

