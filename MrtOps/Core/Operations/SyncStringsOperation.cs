using System.Collections.Generic;
using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

public class SyncStringsOperation : IOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _reportPath;
    private readonly Dictionary<string, Dictionary<string, string>> _localizedStrings;
    private readonly string _backupPath;

    public string Description => _loc.GetString("SyncStringsDesc", Path.GetFileName(_reportPath));

    public SyncStringsOperation(IReportEngine engine, ILocalizationService loc, string reportPath, Dictionary<string, Dictionary<string, string>> localizedStrings)
    {
        _engine = engine;
        _loc = loc;
        _reportPath = reportPath;
        _localizedStrings = localizedStrings;
        _backupPath = _reportPath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_reportPath)) return false;

        File.Copy(_reportPath, _backupPath, true);
        _engine.SyncGlobalizationStrings(_reportPath, _localizedStrings);
        return true;
    }

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