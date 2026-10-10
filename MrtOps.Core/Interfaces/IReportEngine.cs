using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

public interface IReportEngine
{
    void GenerateReport(ReportMetadata metadata, ReportTemplateDef template);
    /// <summary>
    /// Adds a new variable to the specified report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    /// <param name="category">The category of the variable.</param>
    /// <param name="variableName">The name of the variable.</param>
    /// <param name="type">The type of the variable (e.g., 'string', 'int').</param>
    /// <param name="initMethod">The initialization method (e.g., 'Value', 'Expression').</param>
    void AddVariableToReport(string filePath, string category, string variableName, string type = "string", string initMethod = "Value");

    /// <summary>
    /// Removes a variable from the specified report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    /// <param name="variableName">The name of the variable to remove.</param>
    void RemoveVariableFromReport(string filePath, string variableName);

    /// <summary>
    /// Modifies an existing variable in the specified report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    /// <param name="variableName">The current name of the variable.</param>
    /// <param name="newName">The new name for the variable (optional).</param>
    /// <param name="newCategory">The new category for the variable (optional).</param>
    /// <param name="type">The new type for the variable (optional).</param>
    /// <param name="initMethod">The new initialization method for the variable (optional).</param>
    void ModifyVariableInReport(string filePath, string variableName, string? newName = null, string? newCategory = null, string? type = null, string? initMethod = null);

    /// <summary>
    /// Updates a global property of the report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    /// <param name="propertyName">The name of the property to update (e.g., 'Author', 'Description', 'ConvertNulls').</param>
    /// <param name="propertyValue">The new value for the property.</param>
    void UpdateReportProperty(string filePath, string propertyName, string propertyValue);

    /// <summary>
    /// Updates the connection string of a database in the report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    /// <param name="databaseName">The name of the database connection.</param>
    /// <param name="newConnectionString">The new connection string.</param>
    void UpdateDataSourceConnection(string filePath, string databaseName, string newConnectionString);
    
    /// <summary>
    /// Removes unused variables from the report.
    /// </summary>
    /// <param name="filePath">The path to the report file.</param>
    void CleanUnusedVariables(string filePath);

    void ApplyStyleToReport(string reportPath, string styleFilePath);
    void SyncGlobalizationStrings(string reportPath, Dictionary<string, Dictionary<string, string>> localizedStrings);
    bool UpdateReportMetadata(string reportPath, ReportMetadata metadata);
    bool CreateEmptyReport(string outputPath);
}