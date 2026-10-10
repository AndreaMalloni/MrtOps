using System.IO;
using System.Text.Json;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Operations;
using Spectre.Console;

namespace MrtOps.Core;

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

    private string[] GetFiles(string[] targetPaths)
    {
        var files = new HashSet<string>();
        foreach (var path in targetPaths)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.mrt", SearchOption.AllDirectories))
                    files.Add(file);
            }
            else if (File.Exists(path))
            {
                files.Add(path);
            }
        }
        return files.ToArray();
    }

    /// <summary>
    /// Processes a variable operation (add, remove, modify) on a list of files or directories.
    /// </summary>
    public void ProcessVariableOperation(string[] targetPaths, string action, string category, string variableName, string? newName, string? type, string? initMethod, bool dryRun)
    {
        var files = GetFiles(targetPaths);
        if (files.Length == 0)
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", string.Join(", ", targetPaths)));
            return;
        }

        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine($"[yellow]DryRun:[/] {action} var '{variableName}' in {Path.GetFileName(file)}");
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("BatchProcessing"), context =>
        {
            foreach (var file in files)
            {
                IReversibleOperation? operation = action.ToLowerInvariant() switch
                {
                    "add" => new AddVariableOperation(_engine, _loc, file, category, variableName, type ?? "string", initMethod ?? "Value"),
                    "remove" => new RemoveVariableOperation(_engine, _loc, file, variableName),
                    "modify" => new ModifyVariableOperation(_engine, _loc, file, variableName, newName, category, type, initMethod),
                    _ => null
                };

                if (operation != null && _history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }

    /// <summary>
    /// Processes a data source operation on a list of files or directories.
    /// </summary>
    public void ProcessDataSourceOperation(string[] targetPaths, string action, string databaseName, string newConnectionString, bool dryRun)
    {
        var files = GetFiles(targetPaths);
        if (files.Length == 0)
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", string.Join(", ", targetPaths)));
            return;
        }

        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine($"[yellow]DryRun:[/] {action} datasource '{databaseName}' in {Path.GetFileName(file)}");
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("BatchProcessing"), context =>
        {
            foreach (var file in files)
            {
                IReversibleOperation? operation = action.ToLowerInvariant() switch
                {
                    "update-conn" => new UpdateConnectionOperation(_engine, _loc, file, databaseName, newConnectionString),
                    _ => null
                };

                if (operation != null && _history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }

    /// <summary>
    /// Processes a global property operation on a list of files or directories.
    /// </summary>
    public void ProcessPropertyOperation(string[] targetPaths, string action, string propertyName, string propertyValue, bool dryRun)
    {
        var files = GetFiles(targetPaths);
        if (files.Length == 0)
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", string.Join(", ", targetPaths)));
            return;
        }

        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine($"[yellow]DryRun:[/] {action} property '{propertyName}' in {Path.GetFileName(file)}");
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("BatchProcessing"), context =>
        {
            foreach (var file in files)
            {
                IReversibleOperation? operation = action.ToLowerInvariant() switch
                {
                    "set" => new UpdatePropertyOperation(_engine, _loc, file, propertyName, propertyValue),
                    _ => null
                };

                if (operation != null && _history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }

    /// <summary>
    /// Processes a clean operation on a list of files or directories.
    /// </summary>
    public void ProcessCleanOperation(string[] targetPaths, string action, bool dryRun)
    {
        var files = GetFiles(targetPaths);
        if (files.Length == 0)
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", string.Join(", ", targetPaths)));
            return;
        }

        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine($"[yellow]DryRun:[/] {action} in {Path.GetFileName(file)}");
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("BatchProcessing"), context =>
        {
            foreach (var file in files)
            {
                IReversibleOperation? operation = action.ToLowerInvariant() switch
                {
                    "vars" => new CleanUnusedVariablesOperation(_engine, _loc, file),
                    _ => null
                };

                if (operation != null && _history.Execute(operation))
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

    public void ProcessFolderSyncStrings(string folderPath, string stringsFilePath, bool dryRun)
    {
        if (!Directory.Exists(folderPath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFolderNotFound", folderPath));
            return;
        }

        if (!File.Exists(stringsFilePath))
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorFileNotFound", stringsFilePath));
            return;
        }

        Dictionary<string, Dictionary<string, string>>? localizedStrings;
        try
        {
            var jsonContent = File.ReadAllText(stringsFilePath);
            localizedStrings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            if (localizedStrings == null) throw new JsonException();
        }
        catch
        {
            AnsiConsole.MarkupLine(_loc.GetString("ErrorParsingJson", stringsFilePath));
            return;
        }

        var files = Directory.GetFiles(folderPath, "*.mrt", SearchOption.AllDirectories);
        AnsiConsole.MarkupLine(_loc.GetString("FilesFound", files.Length));

        if (dryRun)
        {
            AnsiConsole.MarkupLine(_loc.GetString("DryRunActive"));
            foreach (var file in files)
            {
                AnsiConsole.MarkupLine(_loc.GetString("DryRunSyncStrings", Path.GetFileName(file)));
            }
            return;
        }

        AnsiConsole.Status().Start(_loc.GetString("SyncStringsProcessing"), context =>
        {
            foreach (var file in files)
            {
                var operation = new SyncStringsOperation(_engine, _loc, file, localizedStrings);
                if (_history.Execute(operation))
                {
                    AnsiConsole.MarkupLine(_loc.GetString("Completed", operation.Description));
                }
            }
        });

        AnsiConsole.MarkupLine(_loc.GetString("SuccessProcess"));
    }
}