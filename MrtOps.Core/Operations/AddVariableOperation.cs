using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

public class AddVariableOperation : IOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _category;
    private readonly string _variableName;
    private readonly string _backupPath;

    public string Description => _loc.GetString("AddVarDesc", _variableName, Path.GetFileName(_filePath));

    public AddVariableOperation(IReportEngine engine, ILocalizationService loc, string filePath, string category, string variableName)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _category = category;
        _variableName = variableName;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.AddVariableToReport(_filePath, _category, _variableName);
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