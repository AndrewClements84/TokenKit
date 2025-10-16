using System.Net.Http.Json;
using System.Text.Json;
using TokenKit.Models;

namespace TokenKit.Services;

public class ModelDataScraper
{
    private readonly HttpClient _httpClient;

    public ModelDataScraper(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Fetches models from OpenAI's /v1/models endpoint if a valid API key is provided.
    /// Falls back to local stock data if the key is missing or invalid.
    /// </summary>
    public async Task<List<ModelSpec>> FetchOpenAIModelsAsync(string? apiKey = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("⚠️ No API key provided. Using fallback OpenAI model data.");
            return GetFallbackModels();
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.GetAsync("https://api.openai.com/v1/models");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var models = new List<ModelSpec>();

            foreach (var item in json.GetProperty("data").EnumerateArray())
            {
                var id = item.GetProperty("id").GetString() ?? "";
                if (id.StartsWith("gpt-"))
                {
                    models.Add(new ModelSpec
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

            if (models.Count == 0)
                Console.WriteLine("⚠️ No GPT models found from API. Using fallback data instead.");

            return models.Count > 0 ? models : GetFallbackModels();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to fetch live OpenAI model data: {ex.Message}");
            return GetFallbackModels();
        }
    }

    private static List<ModelSpec> GetFallbackModels() => new()
    {
        new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" },
        new() { Id = "gpt-4o-mini", Provider = "OpenAI", MaxTokens = 64000, InputPricePer1K = 0.002m, OutputPricePer1K = 0.010m, Encoding = "cl100k_base" },
        new() { Id = "gpt-3.5-turbo", Provider = "OpenAI", MaxTokens = 4096, InputPricePer1K = 0.0015m, OutputPricePer1K = 0.002m, Encoding = "cl100k_base" }
    };
}
