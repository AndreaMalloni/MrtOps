using System.Collections.Generic;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for history storage.
/// </summary>
public interface IHistoryStorage
{
    /// <summary>
    /// Saves the history entries.
    /// </summary>
    /// <param name="entries">The list of entries to save.</param>
    void Save(IReadOnlyList<HistoryEntry> entries);

    /// <summary>
    /// Loads the history entries.
    /// </summary>
    /// <returns>A list of history entries.</returns>
    List<HistoryEntry> Load();

    /// <summary>
    /// Clears the history.
    /// </summary>
    void Clear();
}

