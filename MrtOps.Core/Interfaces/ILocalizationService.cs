namespace MrtOps.Core.Interfaces;

/// <summary>
/// Interface for the localization service.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string.
    /// </summary>
    /// <param name="key">The string key.</param>
    /// <param name="args">Optional arguments for string formatting.</param>
    /// <returns>The formatted localized string.</returns>
    string GetString(string key, params object[] args);
}