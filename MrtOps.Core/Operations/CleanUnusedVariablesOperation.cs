using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Operation to clean unused variables from a report.
/// </summary>
public class CleanUnusedVariablesOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _backupPath;

    public string Description => _loc.GetString("CleanUnusedVarsDesc", Path.GetFileName(_filePath));
    public string OperationType => "CleanUnusedVariables";
    public string TargetFilePath => _filePath;
    public string? BackupFilePath => _backupPath;

    public CleanUnusedVariablesOperation(IReportEngine engine, ILocalizationService loc, string filePath)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.CleanUnusedVariables(_filePath);
        return true;
    }

    public bool Undo()
    {
        if (File.Exists(_backupPath))
        {
            File.Copy(_backupPath, _filePath, true);
            File.Delete(_backupPath);
            return true;
        }
        return false;
    }
}
