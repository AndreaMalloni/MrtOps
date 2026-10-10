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

        foreach (var variable in template.DefaultVariables)
        {
            string targetCategory = template.Categories.Count > 0 ? template.Categories[0] : string.Empty;
            report.Dictionary.Variables.Add(new StiVariable(targetCategory, variable, typeof(string), string.Empty, true));
        }

        report.Save(metadata.OutputPath);
    }

    private Type GetTypeFromString(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "int" => typeof(int),
            "decimal" => typeof(decimal),
            "bool" => typeof(bool),
            "datetime" => typeof(DateTime),
            _ => typeof(string)
        };
    }

    private StiVariableInitBy GetInitByFromString(string initMethod)
    {
        if (Enum.TryParse<StiVariableInitBy>(initMethod, true, out var result))
            return result;
        return StiVariableInitBy.Value;
    }

    public void AddVariableToReport(string filePath, string category, string variableName, string type = "string", string initMethod = "Value")
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();

        if (report.Dictionary.Variables[variableName] == null)
        {
            var newVar = new StiVariable(category, variableName, GetTypeFromString(type), string.Empty, true);
            newVar.InitBy = GetInitByFromString(initMethod);
            report.Dictionary.Variables.Add(newVar);
            report.Save(filePath);
        }
    }

    public void RemoveVariableFromReport(string filePath, string variableName)
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();

        var variable = report.Dictionary.Variables[variableName];
        if (variable != null)
        {
            report.Dictionary.Variables.Remove(variable);
            report.Save(filePath);
        }
    }

    public void ModifyVariableInReport(string filePath, string variableName, string? newName = null, string? newCategory = null, string? type = null, string? initMethod = null)
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();

        var variable = report.Dictionary.Variables[variableName];
        if (variable != null)
        {
            bool modified = false;
            
            if (!string.IsNullOrEmpty(newCategory) && variable.Category != newCategory)
            {
                variable.Category = newCategory;
                modified = true;
            }

            if (!string.IsNullOrEmpty(newName) && variable.Name != newName)
            {
                variable.Name = newName;
                variable.Alias = newName;
                modified = true;
            }

            if (!string.IsNullOrEmpty(type))
            {
                var newType = GetTypeFromString(type);
                if (variable.Type != newType)
                {
                    variable.Type = newType;
                    modified = true;
                }
            }

            if (!string.IsNullOrEmpty(initMethod))
            {
                var newInitBy = GetInitByFromString(initMethod);
                if (variable.InitBy != newInitBy)
                {
                    variable.InitBy = newInitBy;
                    modified = true;
                }
            }

            if (modified)
            {
                report.Save(filePath);
            }
        }
    }

    /// <summary>
    /// Updates a global property of the report.
    /// </summary>
    public void UpdateReportProperty(string filePath, string propertyName, string propertyValue)
    {
        var report = new StiReport();
        report.Load(filePath);
        
        bool modified = false;

        switch (propertyName.ToLowerInvariant())
        {
            case "author":
                if (report.ReportAuthor != propertyValue)
                {
                    report.ReportAuthor = propertyValue;
                    modified = true;
                }
                break;
            case "description":
                if (report.ReportDescription != propertyValue)
                {
                    report.ReportDescription = propertyValue;
                    modified = true;
                }
                break;
            case "alias":
                if (report.ReportAlias != propertyValue)
                {
                    report.ReportAlias = propertyValue;
                    modified = true;
                }
                break;
            case "convertnulls":
                if (bool.TryParse(propertyValue, out bool bVal) && report.ConvertNulls != bVal)
                {
                    report.ConvertNulls = bVal;
                    modified = true;
                }
                break;
            case "culture":
                if (report.Culture != propertyValue)
                {
                    report.Culture = propertyValue;
                    modified = true;
                }
                break;
        }

        if (modified)
        {
            report.Save(filePath);
        }
    }

    /// <summary>
    /// Updates the connection string of a database in the report.
    /// </summary>
    public void UpdateDataSourceConnection(string filePath, string databaseName, string newConnectionString)
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();

        bool modified = false;
        var db = report.Dictionary.Databases[databaseName];
        if (db != null)
        {
            var propInfo = db.GetType().GetProperty("ConnectionString");
            if (propInfo != null && propInfo.CanWrite)
            {
                var currentConnectionString = propInfo.GetValue(db) as string;
                if (currentConnectionString != newConnectionString)
                {
                    propInfo.SetValue(db, newConnectionString);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            report.Save(filePath);
        }
    }

    /// <summary>
    /// Removes unused variables from the report by checking if their names appear anywhere else in the report source.
    /// </summary>
    public void CleanUnusedVariables(string filePath)
    {
        var report = new StiReport();
        report.Load(filePath);
        report.Dictionary.Synchronize();
        
        // A simple heuristic for batch cleaning: check if the variable's name appears 
        // in the report's saved string representation more times than just its definition.
        string reportContent = report.SaveToString();
        var variablesToRemove = new List<StiVariable>();

        foreach (StiVariable variable in report.Dictionary.Variables)
        {
            // Not a foolproof AST check, but a safe heuristic for batch operations
            // Count occurrences of the variable name in the raw report string.
            // If it's used in expressions, it will appear multiple times.
            int count = 0;
            int i = 0;
            while ((i = reportContent.IndexOf(variable.Name, i, StringComparison.InvariantCulture)) != -1)
            {
                count++;
                i += variable.Name.Length;
            }

            // Usually defined at least once in the dictionary. If it appears only once, it might be unused.
            if (count <= 1)
            {
                variablesToRemove.Add(variable);
            }
        }

        if (variablesToRemove.Count > 0)
        {
            foreach (var v in variablesToRemove)
            {
                report.Dictionary.Variables.Remove(v);
            }
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