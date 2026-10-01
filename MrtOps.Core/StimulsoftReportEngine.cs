using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Units;

namespace MrtOps.Core;

public class StimulsoftReportEngine : IReportEngine
{
    private readonly ILogger<StimulsoftReportEngine>? _logger;

    public StimulsoftReportEngine(ILogger<StimulsoftReportEngine>? logger = null)
    {
        _logger = logger;
    }

    public void GenerateReport(ReportMetadata metadata, ReportTemplateDef template)
    {
        var report = new StiReport
        {
            ReportName = metadata.Name,
            ReportAlias = metadata.Alias,
            ReportDescription = metadata.Description,
            ReportAuthor = template.Author,
            ConvertNulls = template.ConvertNulls,
            Culture = "it-IT",
            Unit = new StiCentimetersUnit()
        };

        report.Dictionary.Synchronize();

        foreach (var category in template.Categories)
        {
            report.Dictionary.Variables.Add(new StiVariable(category, category));
        }

        foreach (var variable in template.DefaultVariables)
        {
            string targetCategory = template.Categories.Count > 0 ? template.Categories[0] : string.Empty;
            report.Dictionary.Variables.Add(new StiVariable(targetCategory, variable, typeof(string), string.Empty, true));
        }

        report.Save(metadata.OutputPath);
    }

    public void AddVariableToReport(string filePath, string category, string variableName)
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();

        if (report.Dictionary.Variables[category] == null)
        {
            report.Dictionary.Variables.Add(new StiVariable(category, category));
        }

        if (report.Dictionary.Variables[variableName] == null)
        {
            report.Dictionary.Variables.Add(new StiVariable(category, variableName, typeof(string), string.Empty, true));
            report.Save(filePath);
        }
    }

    public void ApplyStyleToReport(string reportPath, string styleFilePath)
    {
        var report = new StiReport();
        report.Load(reportPath);
        report.Styles.Clear();
        report.Styles.Load(styleFilePath);
        report.Save(reportPath);
    }

    public void SyncGlobalizationStrings(string reportPath, Dictionary<string, Dictionary<string, string>> localizedStrings)
    {
        var report = new StiReport();
        report.Load(reportPath);
        report.GlobalizationStrings.Clear();

        foreach (var cultureData in localizedStrings)
        {
            var container = new StiGlobalizationContainer(cultureData.Key);
            foreach (var translation in cultureData.Value)
            {
                var globalizationItem = new StiGlobalizationItem
                {
                    PropertyName = translation.Key,
                    Text = translation.Value
                };
                container.Items.Add(globalizationItem);
            }
            report.GlobalizationStrings.Add(container);
        }

        report.Save(reportPath);
    }

    public bool UpdateReportMetadata(string reportPath, ReportMetadata metadata)
    {
        try
        {
            if (!File.Exists(reportPath))
            {
                _logger?.LogError("Impossibile aggiornare i metadati: il file '{ReportPath}' non esiste.", reportPath);
                return false;
            }

            var report = new StiReport();
            report.Load(reportPath);

            report.ReportName = metadata.Name;
            report.ReportAlias = metadata.Alias;
            report.ReportDescription = metadata.Description;

            report.Save(reportPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Errore durante l'aggiornamento dei metadati per '{ReportPath}'.", reportPath);
            return false;
        }
    }

    public bool CreateEmptyReport(string outputPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(outputPath) || Directory.Exists(outputPath))
            {
                _logger?.LogError("Il percorso specificato '{OutputPath}' non è un percorso di file valido.", outputPath);
                return false;
            }

            string? dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var report = new StiReport
            {
                Culture = "it-IT",
                Unit = new StiCentimetersUnit()
            };
            report.Save(outputPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Errore in Stimulsoft durante la creazione di un report vuoto in '{OutputPath}'.", outputPath);
            return false;
        }
    }
}