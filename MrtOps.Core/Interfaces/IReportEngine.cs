using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for the report generation engine.
/// </summary>
public interface IReportEngine
{
    /// <summary>
    /// Generates a report.
    /// </summary>
    /// <param name="metadata">The report metadata.</param>
    /// <param name="template">The report template definition.</param>
    void GenerateReport(ReportMetadata metadata, ReportTemplateDef template);

    /// <summary>
    /// Adds a variable to the report.
    /// </summary>
    /// <param name="filePath">The report file path.</param>
    /// <param name="category">The variable category.</param>
    /// <param name="variableName">The variable name.</param>
    void AddVariableToReport(string filePath, string category, string variableName);

    /// <summary>
    /// Applies a style to the report.
    /// </summary>
    /// <param name="reportPath">The report path.</param>
    /// <param name="styleFilePath">The style file path.</param>
    void ApplyStyleToReport(string reportPath, string styleFilePath);

    /// <summary>
    /// Synchronizes the report globalization strings.
    /// </summary>
    /// <param name="reportPath">The report path.</param>
    /// <param name="localizedStrings">A dictionary containing the localized strings.</param>
    void SyncGlobalizationStrings(string reportPath, Dictionary<string, Dictionary<string, string>> localizedStrings);

    /// <summary>
    /// Updates the metadata of a report.
    /// </summary>
    /// <param name="reportPath">The report path.</param>
    /// <param name="metadata">The new report metadata.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    bool UpdateReportMetadata(string reportPath, ReportMetadata metadata);

    /// <summary>
    /// Creates an empty report.
    /// </summary>
    /// <param name="outputPath">The output path of the report.</param>
    /// <returns>True if creation was successful, otherwise false.</returns>
    bool CreateEmptyReport(string outputPath);
}