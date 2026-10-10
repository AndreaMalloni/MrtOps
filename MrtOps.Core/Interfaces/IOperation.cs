namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for a generic operation.
/// </summary>
public interface IOperation
{
    /// <summary>
    /// Gets the operation description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <returns>True if execution was successful, otherwise false.</returns>
    bool Execute();

    /// <summary>
    /// Undoes the operation.
    /// </summary>
    /// <returns>True if undo was successful, otherwise false.</returns>
    bool Undo();
}