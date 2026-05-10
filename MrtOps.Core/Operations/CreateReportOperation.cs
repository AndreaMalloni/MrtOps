using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Operations;

public class CreateReportOperation : IOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly ITemplateRepository _templateRepo;
    private readonly ReportMetadata _metadata;
    private readonly ILogger<CreateReportOperation> _logger;

    public string Description => _loc.GetString("OpCreateReport", _metadata.Name, _metadata.TemplateName);

    public CreateReportOperation(
        IReportEngine engine,
        ILocalizationService loc,
        ITemplateRepository templateRepo,
        ReportMetadata metadata,
        ILogger<CreateReportOperation> logger)
    {
        _engine = engine;
        _loc = loc;
        _templateRepo = templateRepo;
        _metadata = metadata;
        _logger = logger;
    }

    public bool Execute()
    {
        try
        {
            string destDir = Path.GetDirectoryName(_metadata.OutputPath);

            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDebug("Creata nuova cartella di destinazione: {Directory}", destDir);
            }

            if (!string.IsNullOrEmpty(_metadata.TemplateName))
            {
                string templatePath = _templateRepo.GetTemplateFilePath(_metadata.TemplateName);
                _logger.LogInformation("Creazione report '{ReportName}' dal template '{TemplateName}' in '{OutputPath}'",
                    _metadata.Name, _metadata.TemplateName, _metadata.OutputPath);

                File.Copy(templatePath, _metadata.OutputPath, overwrite: true);
            }
            else
            {
                _logger.LogInformation("Nessun template specificato. Creazione report vuoto '{ReportName}' in '{OutputPath}'",
                    _metadata.Name, _metadata.OutputPath);

                _engine.CreateEmptyReport(_metadata.OutputPath);
            }

            _engine.UpdateReportMetadata(_metadata.OutputPath, _metadata);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore critico durante la creazione del report '{ReportName}'.", _metadata.Name);
            return false;
        }
    }

    public bool Undo()
    {
        try
        {
            if (File.Exists(_metadata.OutputPath))
            {
                File.Delete(_metadata.OutputPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}