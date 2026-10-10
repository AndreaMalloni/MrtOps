using System.Collections.Generic;

namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface that defines operations for accessing template files.
/// </summary>
public interface ITemplateRepository
{
    /// <summary>
    /// Returns the list of available template names (the .mrt file names without extension).
    /// </summary>
    IEnumerable<string> GetAvailableTemplates();

    /// <summary>
    /// Returns the complete physical path of the requested .mrt template file.
    /// Throws an exception if the template is not found.
    /// </summary>
    string GetTemplateFilePath(string templateName);
}