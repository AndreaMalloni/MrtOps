using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;

namespace MrtOps.Core.Storage;

public class JsonHistoryStorage : IHistoryStorage
{
    private readonly string _storagePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonHistoryStorage(string? storagePath = null)
    {
        _storagePath = storagePath 
                       ?? Environment.GetEnvironmentVariable("MRTOPS_HISTORY_PATH")
                       ?? Path.Combine(Environment.CurrentDirectory, ".mrtops_history.json");
    }

    public void Save(IReadOnlyList<HistoryEntry> entries)
    {
        try
        {
            string? dir = Path.GetDirectoryName(_storagePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(entries, JsonOptions);
            File.WriteAllText(_storagePath, json);
        }
        catch
        {
            // Silently ignore or suppress filesystem serialization issues
        }
    }

    public List<HistoryEntry> Load()
    {
        try
        {
            if (!File.Exists(_storagePath))
                return new List<HistoryEntry>();

            string json = File.ReadAllText(_storagePath);
            var entries = JsonSerializer.Deserialize<List<HistoryEntry>>(json, JsonOptions);
            return entries ?? new List<HistoryEntry>();
        }
        catch
        {
            return new List<HistoryEntry>();
        }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                File.Delete(_storagePath);
            }
        }
        catch
        {
            // Ignored
        }
    }
}

