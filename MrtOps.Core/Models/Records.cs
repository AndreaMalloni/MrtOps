namespace MrtOps.Core.Models;

public record ReportMetadata(string Name, string Alias, string Description, string OutputPath, string TemplateName);
public record DatabaseInfo(string Name, string ConnectionString, string Label);