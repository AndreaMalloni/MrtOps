using System.IO;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Operations;

public class CreateReportOperation : IOperation
{
    private readonly string _path;
    private readonly ReportMetadata _metadata;
    private readonly ReportTemplateDef _template;
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;

    public string Description => _loc.GetString("CreateReportDesc", _path, _template.TemplateName);

    public CreateReportOperation(IReportEngine engine, ILocalizationService loc, ReportMetadata metadata, ReportTemplateDef template)
    {
        _engine = engine;
        _loc = loc;
        _metadata = metadata;
        _template = template;
        _path = metadata.OutputPath;
    }

    public bool Execute()
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        _engine.GenerateReport(_metadata, _template);
        return true;
    }

    public bool Undo()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
            return true;
        }
        return false;
    }
}