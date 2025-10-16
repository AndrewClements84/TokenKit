using System.Text.Json;
using TokenKit.Models;

namespace TokenKit.Registry;

public static class ModelRegistry
{
    private static readonly Lazy<List<ModelSpec>> _models = new(() =>
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;

            // First look in the local output folder (bin/.../Registry/)
            var localPath = Path.Combine(baseDir, "Registry", "models.data.json");

            // If not found, fallback to source-relative path (useful during dev)
            var fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", "models.data.json");

            var filePath = File.Exists(localPath)
                ? localPath
                : File.Exists(fallbackPath)
                    ? fallbackPath
                    : null;

            if (filePath == null)
                throw new FileNotFoundException("Could not locate models.data.json in output or source Registry folder.");

            var json = File.ReadAllText(filePath);
            var models = JsonSerializer.Deserialize<List<ModelSpec>>(json);

            if (models == null || models.Count == 0)
                throw new InvalidDataException("Model registry file is empty or invalid JSON.");

            return models;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to load model registry: {ex.Message}");
            return new List<ModelSpec>();
        }
    });

    /// <summary>
    /// Retrieves a model by ID (case-insensitive).
    /// </summary>
    public static ModelSpec? Get(string id) =>
        _models.Value.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns all registered models.
    /// </summary>
    public static IReadOnlyList<ModelSpec> GetAll() => _models.Value.AsReadOnly();
}
