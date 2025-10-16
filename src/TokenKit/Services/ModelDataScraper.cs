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

    public async Task<List<ModelSpec>> FetchOpenAIModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>("https://api.openai.com/v1/models");
            var models = new List<ModelSpec>();

            foreach (var item in response.GetProperty("data").EnumerateArray())
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

            return models;
        }
        catch
        {
            // fallback for offline/dev
            return new List<ModelSpec>
            {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" },
                new() { Id = "gpt-4o-mini", Provider = "OpenAI", MaxTokens = 64000, InputPricePer1K = 0.002m, OutputPricePer1K = 0.010m, Encoding = "cl100k_base" }
            };
        }
    }

    public async Task<List<ModelSpec>> FetchAnthropicModelsAsync()
    {
        try
        {
            // Anthropic doesn’t have a public model list, so we mock it
            await Task.Delay(200);
            return new List<ModelSpec>
            {
                new() { Id = "claude-3-opus", Provider = "Anthropic", MaxTokens = 200000, InputPricePer1K = 0.008m, OutputPricePer1K = 0.024m, Encoding = "anthropic-v1" },
                new() { Id = "claude-3-sonnet", Provider = "Anthropic", MaxTokens = 160000, InputPricePer1K = 0.003m, OutputPricePer1K = 0.015m, Encoding = "anthropic-v1" }
            };
        }
        catch
        {
            return new List<ModelSpec>();
        }
    }

    public async Task<List<ModelSpec>> FetchAllAsync()
    {
        var openAI = await FetchOpenAIModelsAsync();
        var anthropic = await FetchAnthropicModelsAsync();

        return openAI.Concat(anthropic).ToList();
    }
}

