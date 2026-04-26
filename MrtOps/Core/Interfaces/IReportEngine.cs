using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

public interface IReportEngine
{
    void GenerateReport(ReportMetadata metadata, ReportTemplateDef template);
    void AddVariableToReport(string filePath, string category, string variableName);
    void ApplyStyleToReport(string reportPath, string styleFilePath);
}