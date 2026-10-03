using System.IO;
using System.Text.Json;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Operations;
using Spectre.Console;

namespace MrtOps.Core;

/// <summary>
/// Service for batch processing operations on report files within folders.
/// </summary>
public class BatchProcessingService
{
    private readonly OperationHistoryManager _history;
    private readonly IReportEngine _engine;
    private readonly ILocalizationService _loc;

    /// <summary>
    /// Initializes a new instance of <see cref="BatchProcessingService"/>.
    /// </summary>
    /// <param name="history">Operation history manager.</param>
    /// <param name="engine">Engine for processing reports.</param>
    /// <param name="loc">Localization service.</param>
    public BatchProcessingService(OperationHistoryManager history, IReportEngine engine, ILocalizationService loc)
    {
        _history = history;
        _engine = engine;
        _loc = loc;
    }

    /// <summary>
    /// Processes files in a folder by adding a specified variable.
    /// </summary>
    /// <param name="folderPath">The path of the folder containing the files to process.</param>
    /// <param name="category">The category of the variable to add.</param>
    /// <param name="variableName">The name of the variable to add.</param>
    /// <param name="dryRun">If <c>true</c>, simulates the operation without making actual modifications to the files.</param>
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

    /// <summary>
    /// Processes files in a folder by applying a specified style.
    /// </summary>
    /// <param name="folderPath">The path of the folder containing the files to process.</param>
    /// <param name="styleFilePath">The path to the style file to apply.</param>
    /// <param name="dryRun">If <c>true</c>, simulates the operation without making actual modifications to the files.</param>
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

    /// <summary>
    /// Processes files in a folder by synchronizing localization strings.
    /// </summary>
    /// <param name="folderPath">The path of the folder containing the files to process.</param>
    /// <param name="stringsFilePath">The path of the JSON file containing the localization strings.</param>
    /// <param name="dryRun">If <c>true</c>, simulates the operation without making actual modifications to the files.</param>
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