using System.IO;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Operations;

public class ApplyStyleOperation : IOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly string _reportPath;
    private readonly string _stylePath;
    private readonly string _backupPath;

    public string Description => _loc.GetString("ApplyStyleDesc", Path.GetFileName(_stylePath), Path.GetFileName(_reportPath));

    public ApplyStyleOperation(IReportEngine engine, ILocalizationService loc, string reportPath, string stylePath)
    {
        _engine = engine;
        _loc = loc;
        _reportPath = reportPath;
        _stylePath = stylePath;
        _backupPath = _reportPath + ".bak";
    }

    public bool Execute()
    {
        if (!File.Exists(_reportPath)) return false;

        File.Copy(_reportPath, _backupPath, true);
        _engine.ApplyStyleToReport(_reportPath, _stylePath);
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