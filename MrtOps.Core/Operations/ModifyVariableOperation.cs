using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

public class ModifyVariableOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _variableName;
    private readonly string? _newName;
    private readonly string? _newCategory;
    private readonly string? _type;
    private readonly string? _initMethod;
    private readonly string _backupPath;

    public string Description => _loc.GetString("ModifyVarDesc", _variableName, Path.GetFileName(_filePath));
    public string OperationType => "ModifyVariable";
    public string TargetFilePath => _filePath;
    public string? BackupFilePath => _backupPath;

    public ModifyVariableOperation(IReportEngine engine, ILocalizationService loc, string filePath, string variableName, string? newName = null, string? newCategory = null, string? type = null, string? initMethod = null)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _variableName = variableName;
        _newName = newName;
        _newCategory = newCategory;
        _type = type;
        _initMethod = initMethod;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.ModifyVariableInReport(_filePath, _variableName, _newName, _newCategory, _type, _initMethod);
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
