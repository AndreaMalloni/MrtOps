namespace MrtOps.Core.Interfaces;

public interface ILocalizationService
{
    string GetString(string key, params object[] args);
}