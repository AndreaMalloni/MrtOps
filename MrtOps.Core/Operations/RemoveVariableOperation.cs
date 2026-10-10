using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

public class RemoveVariableOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _variableName;
    private readonly string _backupPath;

    public string Description => _loc.GetString("RemoveVarDesc", _variableName, Path.GetFileName(_filePath));
    public string OperationType => "RemoveVariable";
    public string TargetFilePath => _filePath;
    public string? BackupFilePath => _backupPath;

    public RemoveVariableOperation(IReportEngine engine, ILocalizationService loc, string filePath, string variableName)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _variableName = variableName;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.RemoveVariableFromReport(_filePath, _variableName);
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
