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

    public async Task UpdateAsync(bool useScraper = true)
    {
        List<ModelSpec> updatedModels;

        if (useScraper)
        {
            var scraper = new ModelDataScraper();
            updatedModels = await scraper.FetchAllAsync();
        }
        else
        {
            updatedModels = new()
        {
            new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" }
        };
        }

        var json = JsonSerializer.Serialize(updatedModels, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_dataPath, json);
    }
}
