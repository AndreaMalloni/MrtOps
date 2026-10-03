using System.Collections.Generic;

namespace MrtOps.Core.Models;

/// <summary>
/// Represents the definition of a report template.
/// </summary>
public class ReportTemplateDef
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string TemplateName { get; set; } = "Base";
    
    /// <summary>
    /// Gets or sets the template author.
    /// </summary>
    public string Author { get; set; } = "System";
    
    /// <summary>
    /// Gets or sets a value indicating whether null values should be converted.
    /// </summary>
    public bool ConvertNulls { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the categories associated with the template.
    /// </summary>
    public List<string> Categories { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the default variables for the template.
    /// </summary>
    public List<string> DefaultVariables { get; set; } = new();
}