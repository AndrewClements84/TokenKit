using System.Text.Json;
using TokenKit.Models;

namespace TokenKit.Registry;

public static class ModelRegistry
{
    private static readonly Lazy<List<ModelSpec>> _models = new(() =>
    {
        var json = File.ReadAllText("Registry/models.data.json");
        return JsonSerializer.Deserialize<List<ModelSpec>>(json)!;
    });

    public static ModelSpec? Get(string id) =>
        _models.Value.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

