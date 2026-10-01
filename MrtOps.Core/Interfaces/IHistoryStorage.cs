using System.Collections.Generic;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

public interface IHistoryStorage
{
    void Save(IReadOnlyList<HistoryEntry> entries);
    List<HistoryEntry> Load();
    void Clear();
}

