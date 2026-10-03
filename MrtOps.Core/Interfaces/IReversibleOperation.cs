namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for a reversible operation.
/// </summary>
public interface IReversibleOperation : IOperation
{
    /// <summary>
    /// Gets the operation type.
    /// </summary>
    string OperationType { get; }

    /// <summary>
    /// Gets the target file path.
    /// </summary>
    string TargetFilePath { get; }

    /// <summary>
    /// Gets the backup file path (optional).
    /// </summary>
    string? BackupFilePath { get; }
}

