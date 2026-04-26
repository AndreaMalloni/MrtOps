using System.Collections.Generic;
using MrtOps.Core.Models;

namespace MrtOps.Core.Interfaces;

public interface ITemplateRepository
{
    List<ReportTemplateDef> LoadTemplates();
    void SaveTemplates(List<ReportTemplateDef> templates);
    ReportTemplateDef? GetTemplate(string name);
}