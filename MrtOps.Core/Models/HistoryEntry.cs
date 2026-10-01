using System;

namespace MrtOps.Core.Models;

public record HistoryEntry(
    string OperationType,
    string Description,
    string TargetFilePath,
    string? BackupFilePath,
    DateTime Timestamp
);

