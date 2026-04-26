using System.Collections.Generic;

namespace MrtOps.Core.Models;

public class ReportTemplateDef
{
    public string TemplateName { get; set; } = "Base";
    public string Author { get; set; } = "System";
    public bool ConvertNulls { get; set; } = false;
    public List<string> Categories { get; set; } = new();
    public List<string> DefaultVariables { get; set; } = new();
}