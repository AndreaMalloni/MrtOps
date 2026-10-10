namespace MrtOps.Core.Models;

/// <summary>
/// Represents the metadata of a report.
/// </summary>
/// <param name="Name">The name of the report.</param>
/// <param name="Alias">The alias of the report.</param>
/// <param name="Description">The description of the report.</param>
/// <param name="OutputPath">The output path of the report.</param>
/// <param name="TemplateName">The name of the used template.</param>
public record ReportMetadata(
    string Name,
    string Alias,
    string Description,
    string OutputPath,
    string TemplateName
);

/// <summary>
/// Contains the connection information for a database.
/// </summary>
/// <param name="Name">The name of the database.</param>
/// <param name="ConnectionString">The connection string.</param>
/// <param name="Label">The database label.</param>
public record DatabaseInfo(
    string Name, 
    string ConnectionString, 
    string Label
);