using System;
using System.IO;
using Microsoft.Extensions.Logging;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Operations;

/// <summary>
/// Represents an operation for creating a report.
/// </summary>
public class CreateReportOperation : IReversibleOperation
{
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;
    private readonly ITemplateRepository _templateRepo;
    private readonly ReportMetadata _metadata;
    private readonly ILogger<CreateReportOperation> _logger;

    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    public string Description => _loc.GetString("OpCreateReport", _metadata.Name, _metadata.TemplateName);

    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    public string OperationType => "CreateReport";

    /// <summary>
    /// Gets the path of the target file.
    /// </summary>
    public string TargetFilePath => _metadata.OutputPath;

    /// <summary>
    /// Gets the path of the backup file, if available.
    /// </summary>
    public string? BackupFilePath => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateReportOperation"/> class.
    /// </summary>
    /// <param name="engine">The report engine.</param>
    /// <param name="loc">The localization service.</param>
    /// <param name="templateRepo">The template repository.</param>
    /// <param name="metadata">The report metadata.</param>
    /// <param name="logger">The logger.</param>
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

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <returns><c>true</c> if the execution was successful; otherwise, <c>false</c>.</returns>
    public bool Execute()
    {
        try
        {
            string? destDir = Path.GetDirectoryName(_metadata.OutputPath);

            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDebug("Creata nuova cartella di destinazione: {Directory}", destDir);
            }

            if (!string.IsNullOrEmpty(_metadata.TemplateName))
            {
                string templatePath;
                try
                {
                    templatePath = _templateRepo.GetTemplateFilePath(_metadata.TemplateName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Template '{TemplateName}' non trovato.", _metadata.TemplateName);
                    return false;
                }

                _logger.LogInformation("Creazione report '{ReportName}' dal template '{TemplateName}' in '{OutputPath}'",
                    _metadata.Name, _metadata.TemplateName, _metadata.OutputPath);

                File.Copy(templatePath, _metadata.OutputPath, overwrite: true);
            }
            else
            {
                _logger.LogInformation("Nessun template specificato. Creazione report vuoto '{ReportName}' in '{OutputPath}'",
                    _metadata.Name, _metadata.OutputPath);

                if (!_engine.CreateEmptyReport(_metadata.OutputPath))
                {
                    _logger.LogError("Fallita creazione report vuoto in '{OutputPath}'.", _metadata.OutputPath);
                    return false;
                }
            }

            if (!_engine.UpdateReportMetadata(_metadata.OutputPath, _metadata))
            {
                _logger.LogError("Fallito aggiornamento metadati report in '{OutputPath}'.", _metadata.OutputPath);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore critico durante la creazione del report '{ReportName}'.", _metadata.Name);
            return false;
        }
    }

    /// <summary>
    /// Undoes the operation, removing the created report.
    /// </summary>
    /// <returns><c>true</c> if the undo was successful; otherwise, <c>false</c>.</returns>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il rollback del report '{OutputPath}'.", _metadata.OutputPath);
            return false;
        }
    }
}