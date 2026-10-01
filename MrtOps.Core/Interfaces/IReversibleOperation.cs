namespace MrtOps.Core.Interfaces;

public interface IReversibleOperation : IOperation
{
    string OperationType { get; }
    string TargetFilePath { get; }
    string? BackupFilePath { get; }
}

