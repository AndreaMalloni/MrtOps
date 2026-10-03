using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

/// <summary>
/// Represents an operation to add a variable to a file.
/// </summary>
public class AddVariableOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _filePath;
    private readonly string _category;
    private readonly string _variableName;
    private readonly string _backupPath;

    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    public string Description => _loc.GetString("AddVarDesc", _variableName, Path.GetFileName(_filePath));

    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    public string OperationType => "AddVariable";

    /// <summary>
    /// Gets the path of the target file.
    /// </summary>
    public string TargetFilePath => _filePath;

    /// <summary>
    /// Gets the path of the backup file, if available.
    /// </summary>
    public string? BackupFilePath => _backupPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddVariableOperation"/> class.
    /// </summary>
    /// <param name="engine">The report engine.</param>
    /// <param name="loc">The localization service.</param>
    /// <param name="filePath">The path of the report file.</param>
    /// <param name="category">The category of the variable.</param>
    /// <param name="variableName">The name of the variable.</param>
    public AddVariableOperation(IReportEngine engine, ILocalizationService loc, string filePath, string category, string variableName)
    {
        _engine = engine;
        _loc = loc;
        _filePath = filePath;
        _category = category;
        _variableName = variableName;
        _backupPath = _filePath + ".bak";
    }

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <returns><c>true</c> if the execution was successful; otherwise, <c>false</c>.</returns>
    public bool Execute()
    {
        if (!File.Exists(_filePath)) return false;

        File.Copy(_filePath, _backupPath, true);
        _engine.AddVariableToReport(_filePath, _category, _variableName);
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
            File.Copy(_backupPath, _filePath, true);
            File.Delete(_backupPath);
            return true;
        }
        return false;
    }
}