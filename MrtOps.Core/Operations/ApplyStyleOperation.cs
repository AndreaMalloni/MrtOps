using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Represents an operation to apply a style to a report.
/// </summary>
public class ApplyStyleOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _reportPath;
    private readonly string _stylePath;
    private readonly string _backupPath;

    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    public string Description => _loc.GetString("ApplyStyleDesc", Path.GetFileName(_stylePath), Path.GetFileName(_reportPath));

    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    public string OperationType => "ApplyStyle";

    /// <summary>
    /// Gets the path of the target file.
    /// </summary>
    public string TargetFilePath => _reportPath;

    /// <summary>
    /// Gets the path of the backup file, if available.
    /// </summary>
    public string? BackupFilePath => _backupPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplyStyleOperation"/> class.
    /// </summary>
    /// <param name="engine">The report engine.</param>
    /// <param name="loc">The localization service.</param>
    /// <param name="reportPath">The path of the report to modify.</param>
    /// <param name="stylePath">The path of the style to apply.</param>
    public ApplyStyleOperation(IReportEngine engine, ILocalizationService loc, string reportPath, string stylePath)
    {
        _engine = engine;
        _loc = loc;
        _reportPath = reportPath;
        _stylePath = stylePath;
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
        _engine.ApplyStyleToReport(_reportPath, _stylePath);
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