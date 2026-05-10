using System.Collections.Generic;

namespace MrtOps.Core.Interfaces;

public interface ITemplateRepository
{
    /// <summary>
    /// Restituisce la lista dei nomi dei template disponibili (i nomi dei file .mrt senza estensione).
    /// </summary>
    IEnumerable<string> GetAvailableTemplates();

    /// <summary>
    /// Restituisce il percorso fisico completo del file di template .mrt richiesto.
    /// Lancia un'eccezione se il template non viene trovato.
    /// </summary>
    string GetTemplateFilePath(string templateName);
}