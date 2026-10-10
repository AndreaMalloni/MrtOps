using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Operation to update a global property of a report.
/// </summary>
public class UpdatePropertyOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _propertyName;
    private readonly string _propertyValue;
    private readonly string _backupPath;

    public string Description => _loc.GetString("UpdatePropDesc", _propertyName, Path.GetFileName(_filePath));
    public string OperationType => "UpdateProperty";
    public string TargetFilePath => _filePath;
    public string? BackupFilePath => _backupPath;

    public UpdatePropertyOperation(IReportEngine engine, ILocalizationService loc, string filePath, string propertyName, string propertyValue)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _propertyName = propertyName;
        _propertyValue = propertyValue;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.UpdateReportProperty(_filePath, _propertyName, _propertyValue);
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
