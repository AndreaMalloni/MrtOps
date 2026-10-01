using System;
using System.IO;

namespace MrtOps.Core;

public static class ReportPathResolver
{
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

