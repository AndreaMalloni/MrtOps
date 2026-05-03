using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

public class JsonTemplateRepository : ITemplateRepository
{
    private readonly string _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates.json");

    public List<ReportTemplateDef> LoadTemplates()
    {
        if (!File.Exists(_storagePath))
        {
            var defaultTemplates = new List<ReportTemplateDef>
            {
                new ReportTemplateDef { TemplateName = "Empty" },
                new ReportTemplateDef {
                    TemplateName = "Standard_Template",
                    Author = "Organization",
                    Categories = new List<string> { "ParameterFields", "FormulaFields" },
                    DefaultVariables = new List<string> { "Module" }
                }
            };
            SaveTemplates(defaultTemplates);
            return defaultTemplates;
        }
        var jsonContent = File.ReadAllText(_storagePath);
        return JsonSerializer.Deserialize<List<ReportTemplateDef>>(jsonContent) ?? new List<ReportTemplateDef>();
    }

    public void SaveTemplates(List<ReportTemplateDef> templates)
    {
        var jsonContent = JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_storagePath, jsonContent);
    }

    public ReportTemplateDef? GetTemplate(string name)
    {
        return LoadTemplates().Find(template => template.TemplateName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}