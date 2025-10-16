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

    public async Task UpdateAsync(string? openAiApiKey = null)
    {
        var scraper = new ModelDataScraper();
        var updatedModels = await scraper.FetchOpenAIModelsAsync(openAiApiKey);

        var json = JsonSerializer.Serialize(updatedModels, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_dataPath, json);
    }
}
