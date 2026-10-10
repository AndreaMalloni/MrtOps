using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Operation to update a connection string of a database in a report.
/// </summary>
public class UpdateConnectionOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _databaseName;
    private readonly string _connectionString;
    private readonly string _backupPath;

    public string Description => _loc.GetString("UpdateConnDesc", _databaseName, Path.GetFileName(_filePath));
    public string OperationType => "UpdateConnection";
    public string TargetFilePath => _filePath;
    public string? BackupFilePath => _backupPath;

    public UpdateConnectionOperation(IReportEngine engine, ILocalizationService loc, string filePath, string databaseName, string connectionString)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _databaseName = databaseName;
        _connectionString = connectionString;
        _backupPath = _filePath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.UpdateDataSourceConnection(_filePath, _databaseName, _connectionString);
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
