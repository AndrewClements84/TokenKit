using System.Net.Http.Json;
using System.Text.Json;
using TokenKit.CLI;
using TokenKit.Core.Models;

namespace TokenKit.Services;

public class ModelDataScraper
{
    private readonly HttpClient _httpClient;

    public ModelDataScraper(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<List<ModelInfo>> FetchOpenAIModelsAsync(string? apiKey = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ConsoleStyler.WriteWarning("⚠️ No API key provided. Using fallback OpenAI model data.");
            return GetFallbackModels();
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.GetAsync("https://api.openai.com/v1/models");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var models = new List<ModelInfo>();

            foreach (var item in json.GetProperty("data").EnumerateArray())
            {
                var id = item.GetProperty("id").GetString() ?? "";
                if (id.StartsWith("gpt-"))
                {
                    models.Add(new ModelInfo
                    {
                        Id = id,
                        Provider = "OpenAI",
                        MaxTokens = id.Contains("4o") ? 128000 : 8192,
                        InputPricePer1K = 0.005m,
                        OutputPricePer1K = 0.015m,
                        Encoding = "cl100k_base"
                    });
                }
            }

            return models.Count > 0 ? models : GetFallbackModels();
        }
        catch (Exception ex)
        {
            ConsoleStyler.WriteError($"❌ Failed to fetch live OpenAI model data: {ex.Message}");
            return GetFallbackModels();
        }
    }

    private static List<ModelInfo> GetFallbackModels() => new()
    {
        new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" },
        new() { Id = "gpt-4o-mini", Provider = "OpenAI", MaxTokens = 64000, InputPricePer1K = 0.002m, OutputPricePer1K = 0.010m, Encoding = "cl100k_base" },
        new() { Id = "gpt-3.5-turbo", Provider = "OpenAI", MaxTokens = 4096, InputPricePer1K = 0.0015m, OutputPricePer1K = 0.002m, Encoding = "cl100k_base" }
    };
}
