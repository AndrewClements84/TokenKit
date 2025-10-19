using System.Text.Json;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Core.Implementations;

public sealed class JsonModelRegistry : IModelRegistry
{
    private readonly List<ModelInfo> _models;

    /// <param name="path">
    /// Optional custom path to models.data.json.
    /// If null, resolves to: AppContext.BaseDirectory/models.data.json
    /// </param>
    public JsonModelRegistry(string? path = null)
    {
        var file = path ?? Path.Combine(AppContext.BaseDirectory, "models.data.json");

        if (File.Exists(file))
        {
            try
            {
                var json = File.ReadAllText(file);
                _models = JsonSerializer.Deserialize<List<ModelInfo>>(json) ?? new();
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                // Gracefully handle corrupt or unreadable JSON
                _models = new List<ModelInfo>();
            }
        }
        else
        {
            _models = new List<ModelInfo>();
        }
    }

    public ModelInfo? Get(string id) =>
        _models.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ModelInfo> GetAll(string? provider = null)
    {
        if (string.IsNullOrWhiteSpace(provider)) return _models;
        return _models
            .Where(m => string.Equals(m.Provider, provider, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

