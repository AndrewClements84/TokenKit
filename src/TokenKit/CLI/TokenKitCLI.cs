using System.Text.Json;
using TokenKit.Models;
using TokenKit.Services;
using TokenKit.Registry;

namespace TokenKit.CLI;

public static class TokenKitCLI
{
    public static async Task RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var command = args[0].ToLowerInvariant();
        switch (command)
        {
            case "analyze":
                await AnalyzeAsync(args);
                break;
            case "validate":
                await ValidateAsync(args);
                break;
            case "update-models":
                await UpdateModelsAsync();
                break;
            case "scrape-models":
                await ScrapeModelsAsync();
                break;
            case "--help":
            case "-h":
            default:
                ShowHelp();
                break;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
        🧠 TokenKit CLI — Tokenization & Validation Toolkit for LLMs
        ------------------------------------------------------------
        Easily analyze, validate, and update model metadata from the terminal.

        📘 Usage:
          tokenkit analyze "<text | path>" --model <model-id>
          tokenkit validate "<text | path>" --model <model-id>
          tokenkit update-models [--from-stdin]

        💡 Input Options:
          • Inline text  →  tokenkit analyze "Hello world!" --model gpt-4o
          • From file    →  tokenkit analyze prompt.txt --model claude-3
          • From stdin   →  echo "Hello world!" | tokenkit analyze --model gpt-4o

        🔄 Model Updates:
          • Fetch or replace model data:
              tokenkit update-models
          • Scrape latest model data (without saving):
              tokenkit scrape-models
          • Pipe JSON directly (stdin):
              cat newmodels.json | tokenkit update-models

        🧩 Examples:
          tokenkit analyze "Write a haiku about energy efficiency" --model gpt-4o
          tokenkit validate prompt.txt --model claude-3
          echo "The quick brown fox" | tokenkit analyze --model gpt-4o

        🧾 Output:
          JSON-formatted summary including token count, estimated cost, and validation status.

        ------------------------------------------------------------
        Version 1.0.0  |  © 2025 Flow Labs
        """);
    }

    private static async Task AnalyzeAsync(string[] args)
    {
        // Detect piped input
        string? pipedInput = null;
        if (Console.IsInputRedirected)
        {
            pipedInput = await Console.In.ReadToEndAsync();
            pipedInput = pipedInput.Trim();
        }

        var modelFlagIndex = Array.IndexOf(args, "--model");
        var modelId = (modelFlagIndex >= 0 && modelFlagIndex + 1 < args.Length)
            ? args[modelFlagIndex + 1]
            : "gpt-4o";

        string text;

        if (!string.IsNullOrWhiteSpace(pipedInput))
        {
            text = pipedInput;
        }
        else if (args.Length > 1)
        {
            var inputParts = (modelFlagIndex > 0 ? args[1..modelFlagIndex] : args[1..]);
            var inputArg = string.Join(" ", inputParts);
            text = File.Exists(inputArg)
                ? await File.ReadAllTextAsync(inputArg)
                : inputArg;
        }
        else
        {
            Console.WriteLine("❌ Missing text, file path, or stdin input.");
            return;
        }

        var tokenizer = new TokenizerService();
        var result = tokenizer.Analyze(text, modelId);

        var model = ModelRegistry.Get(modelId);
        if (model == null)
        {
            Console.WriteLine($"❌ Model '{modelId}' not found in registry.");
            return;
        }

        var cost = CostEstimator.Estimate(model, result.TokenCount);

        var output = new
        {
            Model = model.Id,
            Provider = model.Provider,
            result.TokenCount,
            EstimatedCost = cost,
            Valid = result.TokenCount <= model.MaxTokens
        };

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task ValidateAsync(string[] args)
    {
        string? pipedInput = null;
        if (Console.IsInputRedirected)
        {
            pipedInput = await Console.In.ReadToEndAsync();
            pipedInput = pipedInput.Trim();
        }

        var modelFlagIndex = Array.IndexOf(args, "--model");
        var modelId = (modelFlagIndex >= 0 && modelFlagIndex + 1 < args.Length)
            ? args[modelFlagIndex + 1]
            : "gpt-4o";

        string text;

        if (!string.IsNullOrWhiteSpace(pipedInput))
        {
            text = pipedInput;
        }
        else if (args.Length > 1)
        {
            var inputParts = (modelFlagIndex > 0 ? args[1..modelFlagIndex] : args[1..]);
            var inputArg = string.Join(" ", inputParts);
            text = File.Exists(inputArg)
                ? await File.ReadAllTextAsync(inputArg)
                : inputArg;
        }
        else
        {
            Console.WriteLine("❌ Missing text, file path, or stdin input.");
            return;
        }

        var tokenizer = new TokenizerService();
        var result = tokenizer.Analyze(text, modelId);
        var model = ModelRegistry.Get(modelId)!;

        var validation = new ValidationService().Validate(model, result.TokenCount);
        Console.WriteLine(JsonSerializer.Serialize(validation, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task UpdateModelsAsync()
    {
        string? pipedInput = null;
        if (Console.IsInputRedirected)
        {
            pipedInput = await Console.In.ReadToEndAsync();
            pipedInput = pipedInput.Trim();
        }

        var updater = new ModelDataUpdater("Registry/models.data.json");

        if (!string.IsNullOrWhiteSpace(pipedInput))
        {
            try
            {
                var models = JsonSerializer.Deserialize<List<ModelSpec>>(pipedInput);
                if (models is not null)
                {
                    var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync("Registry/models.data.json", json);
                    Console.WriteLine($"✅ Updated model registry with {models.Count} entries from stdin.");
                    return;
                }

                Console.WriteLine("⚠️ No valid model data found in stdin input.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to parse JSON input: {ex.Message}");
            }
        }
        else
        {
            await updater.UpdateAsync();
            Console.WriteLine("✅ Model data updated successfully from default source.");
        }
    }

    private static async Task ScrapeModelsAsync()
    {
        Console.WriteLine("🔍 Fetching latest model data from providers...");

        try
        {
            var scraper = new ModelDataScraper();
            var models = await scraper.FetchAllAsync();

            if (models.Count == 0)
            {
                Console.WriteLine("⚠️ No models retrieved (possibly offline or API unavailable).");
                return;
            }

            Console.WriteLine($"✅ Retrieved {models.Count} models:");
            foreach (var m in models)
                Console.WriteLine($"  - {m.Provider}: {m.Id} ({m.MaxTokens} tokens)");

            Console.WriteLine("\nTo save these models locally, run:");
            Console.WriteLine("  tokenkit update-models");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to scrape models: {ex.Message}");
        }
    }
}
