using System;
using System.IO;

namespace MrtOps.Core;

/// <summary>
/// Resolves paths for reports providing a full normalized path and a name for the report.
/// </summary>
public static class ReportPathResolver
{
    /// <summary>
    /// Resolves the full path and the effective name of the report starting from an input path and an optional name.
    /// </summary>
    /// <param name="inputPath">Input path, can be a folder or a file.</param>
    /// <param name="reportName">Optional report name.</param>
    /// <returns>A tuple containing the full path and the effective name of the report.</returns>
    public static (string FullPath, string EffectiveReportName) Resolve(string? inputPath, string? reportName)
    {
        string rawPath = string.IsNullOrWhiteSpace(inputPath) ? ".\\" : inputPath.Trim();

        bool isExplicitDirectory = rawPath.EndsWith('/') 
                                   || rawPath.EndsWith('\\') 
                                   || rawPath == "." 
                                   || rawPath == ".." 
                                   || Directory.Exists(rawPath);

        string effectiveName;

        if (!string.IsNullOrWhiteSpace(reportName))
        {
            effectiveName = reportName.Trim();
        }
        else
        {
            if (!isExplicitDirectory && Path.HasExtension(rawPath))
            {
                effectiveName = Path.GetFileNameWithoutExtension(rawPath);
            }
            else
            {
                effectiveName = "Report";
            }
        }

        string fullPath;

        if (isExplicitDirectory)
        {
            fullPath = Path.Combine(rawPath, $"{effectiveName}.mrt");
        }
        else
        {
            if (rawPath.EndsWith(".mrt", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = rawPath;
            }
            else
            {
                fullPath = rawPath + ".mrt";
            }
        }

        return (Path.GetFullPath(fullPath), effectiveName);
    }
}

