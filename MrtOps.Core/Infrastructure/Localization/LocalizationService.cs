using System.Collections.Generic;
using System.Globalization;
using MrtOps.Core.Interfaces;

namespace MrtOps.Infrastructure.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, string> _strings;

    public LocalizationService()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _strings = culture == "it" ? GetItalianStrings() : GetEnglishStrings();
    }

    public string GetString(string key, params object[] args)
    {
        if (!_strings.TryGetValue(key, out var value))
        {
            value = $"[{key}]";
        }
        return args.Length > 0 ? string.Format(value, args) : value;
    }

    private Dictionary<string, string> GetEnglishStrings() => new()
    {
        { "ErrorFolderNotFound", "[red]Error:[/] Directory '{0}' not found." },
        { "ErrorFileNotFound", "[red]Error:[/] File '{0}' not found." },
        { "ErrorTemplateNotFound", "[red]Error:[/] Template '{0}' not found." },
        { "FilesFound", "Found [bold yellow]{0}[/] .mrt files." },
        { "DryRunActive", "[bold cyan]--- DRY RUN MODE ACTIVE ---[/]" },
        { "DryRunAddVar", "- Target modification: '{0}' on [grey]{1}[/]" },
        { "DryRunApplyStyle", "- Style overwrite target: [grey]{0}[/]" },
        { "BatchProcessing", "Processing batch..." },
        { "SyncProcessing", "Synchronizing styles..." },
        { "Completed", "[grey]Completed:[/] {0}" },
        { "SuccessProcess", "[bold green]✓ Processing Completed![/]" },
        { "SuccessReport", "[green]Success![/] Report generated: [cyan]{0}[/]" },
        { "PromptPath", "File path:" },
        { "PromptName", "Report name:" },
        { "PromptTemplate", "Template:" },
        { "PromptFolder", "Target directory:" },
        { "PromptVar", "Variable to add:" },
        { "PromptStyle", "Style file path (.sts):" },
        { "UndoSuccess", "[yellow]Rollback completed:[/] {0}" },
        { "UndoFail", "[red]No operations to undo in the current session.[/]" },
        { "CreateReportDesc", "Report created at '{0}' using template '{1}'" },
        { "AddVarDesc", "Added variable '{0}' to report '{1}'" },
        { "ApplyStyleDesc", "Applied style '{0}' to report '{1}'" },
        { "PromptServer", "Server address (es. localhost or .\\SQLEXPRESS):" },
        { "PromptAuth", "Use Windows Authentication?" },
        { "PromptUser", "Username:" },
        { "PromptPass", "Password:" },
        { "ScanningDatabases", "Scanning databases on server..." },
        { "DbListHeader", "Available Databases on {0}:" },
        { "ConnectionStringLabel", "Connection String:" },
    };

    private Dictionary<string, string> GetItalianStrings() => new()
    {
        { "ErrorFolderNotFound", "[red]Errore:[/] Cartella '{0}' non trovata." },
        { "ErrorFileNotFound", "[red]Errore:[/] File '{0}' non trovato." },
        { "ErrorTemplateNotFound", "[red]Errore:[/] Template '{0}' non trovato." },
        { "FilesFound", "Trovati [bold yellow]{0}[/] file .mrt." },
        { "DryRunActive", "[bold cyan]--- MODALITÀ DRY RUN ATTIVA ---[/]" },
        { "DryRunAddVar", "- Modifica prevista: '{0}' su [grey]{1}[/]" },
        { "DryRunApplyStyle", "- Sovrascrittura stile prevista su: [grey]{0}[/]" },
        { "BatchProcessing", "Elaborazione batch in corso..." },
        { "SyncProcessing", "Sincronizzazione stili in corso..." },
        { "Completed", "[grey]Completato:[/] {0}" },
        { "SuccessProcess", "[bold green]✓ Elaborazione Completata![/]" },
        { "SuccessReport", "[green]Successo![/] Report generato: [cyan]{0}[/]" },
        { "PromptPath", "Percorso file:" },
        { "PromptName", "Nome report:" },
        { "PromptTemplate", "Template:" },
        { "PromptFolder", "Cartella target:" },
        { "PromptVar", "Variabile da aggiungere:" },
        { "PromptStyle", "Percorso file stile (.sts):" },
        { "UndoSuccess", "[yellow]Rollback completato:[/] {0}" },
        { "UndoFail", "[red]Nessuna operazione da annullare nella sessione corrente.[/]" },
        { "CreateReportDesc", "Creazione report in '{0}' con template '{1}'" },
        { "AddVarDesc", "Aggiunta variabile '{0}' al report '{1}'" },
        { "ApplyStyleDesc", "Applicato stile '{0}' al report '{1}'" },
        { "PromptServer", "Indirizzo server (es. localhost o .\\SQLEXPRESS):" },
        { "PromptAuth", "Usare Windows Authentication?" },
        { "PromptUser", "Username:" },
        { "PromptPass", "Password:" },
        { "ScanningDatabases", "Scansione dei database sul server in corso..." },
        { "DbListHeader", "Database disponibili su {0}:" },
        { "ConnectionStringLabel", "Stringa di Connessione:" },
    };
}