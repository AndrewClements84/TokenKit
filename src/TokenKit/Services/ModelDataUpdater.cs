using System.Text.Json;
using TokenKit.Models;

namespace TokenKit.Services;

public class ModelDataUpdater
{
    private readonly string _dataPath;

    public ModelDataUpdater(string dataPath)
    {
        // Always resolve relative to the assembly folder
        var baseDir = AppContext.BaseDirectory;
        _dataPath = Path.Combine(baseDir, dataPath);

        var directory = Path.GetDirectoryName(_dataPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);
    }

    public async Task UpdateAsync()
    {
        // Example: dummy new model data
        var updatedModels = new List<ModelSpec>
        {
            new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" },
            new() { Id = "claude-3-opus", Provider = "Anthropic", MaxTokens = 200000, InputPricePer1K = 0.008m, OutputPricePer1K = 0.024m, Encoding = "anthropic-v1" }
        };

        var json = JsonSerializer.Serialize(updatedModels, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_dataPath, json);
    }
}
