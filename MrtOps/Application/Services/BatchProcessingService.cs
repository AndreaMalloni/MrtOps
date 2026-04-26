using System.IO;
using MrtOps.Core.History;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Operations;
using Spectre.Console;

namespace MrtOps.Application.Services;

public class BatchProcessingService
{
    private readonly OperationHistoryManager _history;
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;

    public BatchProcessingService(OperationHistoryManager history, IReportEngine engine, ILocalizationService loc)
    {
        _history = history;
        _engine = engine;
        _loc = loc;
    }

    public void ProcessFolderAddVariable(string folderPath, string category, string variableName, bool dryRun)
    {
        if (!Directory.Exists(folderPath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFolderNotFound", folderPath));
            return;
        }

        var files = Directory.GetFiles(folderPath, "*.mrt", SearchOption.AllDirectories);
        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine(_loc.GetString("DryRunAddVar", variableName, Path.GetFileName(file)));
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("BatchProcessing"), context =>
        {
            foreach (var file in files)
            {
                var operation = new AddVariableOperation(_engine, _loc, file, category, variableName);
                if (_history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }

    public void ProcessFolderApplyStyle(string folderPath, string styleFilePath, bool dryRun)
    {
        if (!Directory.Exists(folderPath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFolderNotFound", folderPath));
            return;
        }

        if (!File.Exists(styleFilePath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", styleFilePath));
            return;
        }

        var files = Directory.GetFiles(folderPath, "*.mrt", SearchOption.AllDirectories);
        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine(_loc.GetString("DryRunApplyStyle", Path.GetFileName(file)));
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("SyncProcessing"), context =>
        {
            foreach (var file in files)
            {
                var operation = new ApplyStyleOperation(_engine, _loc, file, styleFilePath);
                if (_history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }
}