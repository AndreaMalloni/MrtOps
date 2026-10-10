using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Storage;

/// <summary>
/// Provides access to templates stored in the local file system.
/// </summary>
public class FileTemplateRepository : ITemplateRepository
{
    private readonly string _templatesDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTemplateRepository"/> class.
    /// </summary>
    public FileTemplateRepository()
    {
        _templatesDirectory = Environment.GetEnvironmentVariable("MRTOPS_TEMPLATES_DIR")
                              ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
        }
    }

    /// <summary>
    /// Gets a list of available templates.
    /// </summary>
    /// <returns>A collection of available template names.</returns>
    public IEnumerable<string> GetAvailableTemplates()
    {
        if (!Directory.Exists(_templatesDirectory)) return Enumerable.Empty<string>();

        return Directory.GetFiles(_templatesDirectory, "*.mrt")
                        .Select(Path.GetFileNameWithoutExtension)!;
    }

    /// <summary>
    /// Retrieves the full file path of the specified template.
    /// </summary>
    /// <param name="templateName">The name of the template to search for.</param>
    /// <returns>The path of the template file.</returns>
    public string GetTemplateFilePath(string templateName)
    {
        string filePath = Path.Combine(_templatesDirectory, $"{templateName}.mrt");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Il template specificato '{templateName}' non è stato trovato nel percorso: {filePath}");
        }

        return filePath;
    }
}