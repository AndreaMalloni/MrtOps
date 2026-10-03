using System.Collections.Generic;
using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Represents an operation to synchronize globalization strings in a report.
/// </summary>
public class SyncStringsOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _reportPath;
    private readonly Dictionary<string, Dictionary<string, string>> _localizedStrings;
    private readonly string _backupPath;

    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    public string Description => _loc.GetString("SyncStringsDesc", Path.GetFileName(_reportPath));

    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    public string OperationType => "SyncStrings";

    /// <summary>
    /// Gets the path of the target file.
    /// </summary>
    public string TargetFilePath => _reportPath;

    /// <summary>
    /// Gets the path of the backup file, if available.
    /// </summary>
    public string? BackupFilePath => _backupPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncStringsOperation"/> class.
    /// </summary>
    /// <param name="engine">The report engine.</param>
    /// <param name="loc">The localization service.</param>
    /// <param name="reportPath">The path of the report file.</param>
    /// <param name="localizedStrings">The localized strings to synchronize.</param>
    public SyncStringsOperation(IReportEngine engine, ILocalizationService loc, string reportPath, Dictionary<string, Dictionary<string, string>> localizedStrings)
    {
        _engine = engine;
        _loc = loc;
        _reportPath = reportPath;
        _localizedStrings = localizedStrings;
        _backupPath = _reportPath + ".bak";
    }

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <returns><c>true</c> if the execution was successful; otherwise, <c>false</c>.</returns>
    public bool Execute()
    {
        if (!File.Exists(_reportPath)) return false;

        File.Copy(_reportPath, _backupPath, true);
        _engine.SyncGlobalizationStrings(_reportPath, _localizedStrings);
        return true;
    }

    /// <summary>
    /// Undoes the operation, restoring the previous state from the backup.
    /// </summary>
    /// <returns><c>true</c> if the undo was successful; otherwise, <c>false</c>.</returns>
    public bool Undo()
    {
        if (File.Exists(_backupPath))
        {
            File.Copy(_backupPath, _reportPath, true);
            File.Delete(_backupPath);
            return true;
        }
        return false;
    }
}