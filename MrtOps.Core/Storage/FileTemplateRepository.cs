using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MrtOps.Core.Interfaces;

namespace MrtOps.Core.Storage;

public class FileTemplateRepository : ITemplateRepository
{
    private readonly string _templatesDirectory;

    public FileTemplateRepository()
    {
        _templatesDirectory = Environment.GetEnvironmentVariable("MRTOPS_TEMPLATES_DIR")
                              ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
        }
    }

    public IEnumerable<string> GetAvailableTemplates()
    {
        if (!Directory.Exists(_templatesDirectory)) return Enumerable.Empty<string>();

        return Directory.GetFiles(_templatesDirectory, "*.mrt")
                        .Select(Path.GetFileNameWithoutExtension)!;
    }

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