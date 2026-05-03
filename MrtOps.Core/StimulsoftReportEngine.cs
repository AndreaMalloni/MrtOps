using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Units;

namespace MrtOps.Core;

public class StimulsoftReportEngine : IReportEngine
{
    public void GenerateReport(ReportMetadata metadata, ReportTemplateDef template)
    {
        var report = new StiReport();
        report.ReportName = metadata.Name;
        report.ReportAlias = metadata.Alias;
        report.ReportDescription = metadata.Description;
        report.ReportAuthor = template.Author;
        report.ConvertNulls = template.ConvertNulls;
        report.Culture = "it-IT";
        report.Unit = new StiCentimetersUnit();

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
}