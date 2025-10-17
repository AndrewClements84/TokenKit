using System.Text.Json;
using System.Diagnostics;
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
                await UpdateModelsAsync(args);
                break;
            case "scrape-models":
                await ScrapeModelsAsync(args);
                break;
            case "models":
                await HandleModelsCommandAsync(args);
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
        Analyze, validate, or update model metadata directly from your terminal.

        📘 Usage:
          tokenkit analyze "<text | path>" --model <model-id> [--engine <engine>]
          tokenkit validate "<text | path>" --model <model-id> [--engine <engine>]
          tokenkit update-models [--openai-key <key>]
          tokenkit scrape-models [--openai-key <key>]
          tokenkit models list

        💡 Input Options:
          • Inline text  →  tokenkit analyze "Hello world!" --model gpt-4o
          • From file    →  tokenkit analyze prompt.txt --model gpt-4o
          • From stdin   →  echo "Hello world!" | tokenkit analyze --model gpt-4o

        ⚙️ Encoder Options:
          • Built-in (default):       --engine simple
          • SharpToken (TikToken):    --engine sharptoken
          • Microsoft ML Tokenizers:  --engine mltokenizers

        🔍 Model Commands:
          • List all models in registry:
              tokenkit models list
          • Filter by provider (coming soon):
              tokenkit models list --provider OpenAI

        ------------------------------------------------------------
        Version 1.0.0  |  © 2025 Flow Labs
        """);
    }

    // -----------------------------
    // NEW: Models command handling
    // -----------------------------
    private static async Task HandleModelsCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("❌ Missing subcommand. Use: tokenkit models list");
            return;
        }

        var subcommand = args[1].ToLowerInvariant();
        switch (subcommand)
        {
            case "list":
                string? providerFilter = null;
                var providerIndex = Array.IndexOf(args, "--provider");
                if (providerIndex >= 0 && providerIndex + 1 < args.Length)
                    providerFilter = args[providerIndex + 1];

                ListModels(providerFilter);
                break;

            default:
                Console.WriteLine($"❌ Unknown subcommand '{subcommand}'. Try: tokenkit models list");
                break;
        }

        await Task.CompletedTask;
    }

    private static void ListModels(string? providerFilter = null)
    {
        try
        {
            var models = ModelRegistry.GetAll();
            if (models == null || models.Count == 0)
            {
                Console.WriteLine("⚠️ No models found in registry.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(providerFilter))
            {
                models = models
                    .Where(m => m.Provider.Equals(providerFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (models.Count == 0)
                {
                    Console.WriteLine($"⚠️ No models found for provider '{providerFilter}'.");
                    return;
                }
            }

            Console.WriteLine("📦 Registered Models:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"{"Provider",-12} {"Model ID",-22} {"MaxTokens",10} {"Input/Output (per 1K)",30}");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var m in models)
            {
                Console.WriteLine(
                    $"{m.Provider,-12} {m.Id,-22} {m.MaxTokens,10:N0}  " +
                    $"${m.InputPricePer1K:F4}/${m.OutputPricePer1K:F4}");
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"Total: {models.Count} {(providerFilter != null ? $"({providerFilter})" : "")} models\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to list models: {ex.Message}");
        }
    }

    // -----------------------------
    // Existing functionality
    // -----------------------------

    private static async Task AnalyzeAsync(string[] args)
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

        // Detect optional --engine flag
        var engineFlagIndex = Array.IndexOf(args, "--engine");
        var engineName = (engineFlagIndex >= 0 && engineFlagIndex + 1 < args.Length)
            ? args[engineFlagIndex + 1]
            : "simple";

        Debug.WriteLine($"[TokenKitCLI] Selected engine: {engineName}");

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

        var tokenizer = new TokenizerService(engineName);
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
            Engine = result.Engine,
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

        // Detect optional --engine flag
        var engineFlagIndex = Array.IndexOf(args, "--engine");
        var engineName = (engineFlagIndex >= 0 && engineFlagIndex + 1 < args.Length)
            ? args[engineFlagIndex + 1]
            : "simple";

        Debug.WriteLine($"[TokenKitCLI] Selected engine: {engineName}");

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

        var tokenizer = new TokenizerService(engineName);
        var result = tokenizer.Analyze(text, modelId);
        var model = ModelRegistry.Get(modelId)!;

        var validation = new ValidationService().Validate(model, result.TokenCount);

        var output = new
        {
            Model = model.Id,
            Provider = model.Provider,
            result.TokenCount,
            Engine = result.Engine,
            validation.IsValid,
            validation.Message
        };

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task UpdateModelsAsync(string[] args)
    {
        string? pipedInput = null;
        if (Console.IsInputRedirected)
        {
            pipedInput = await Console.In.ReadToEndAsync();
            pipedInput = pipedInput.Trim();
        }

        string? openAiKey = null;
        var keyIndex = Array.IndexOf(args, "--openai-key");
        if (keyIndex >= 0 && keyIndex + 1 < args.Length)
            openAiKey = args[keyIndex + 1];

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
            await updater.UpdateAsync(openAiKey);
            Console.WriteLine("✅ Model data updated successfully from available source.");
        }
    }

    private static async Task ScrapeModelsAsync(string[] args)
    {
        Console.WriteLine("🔍 Fetching latest OpenAI model data...");

        string? openAiKey = null;
        var keyIndex = Array.IndexOf(args, "--openai-key");
        if (keyIndex >= 0 && keyIndex + 1 < args.Length)
            openAiKey = args[keyIndex + 1];

        try
        {
            var scraper = new ModelDataScraper();
            var models = await scraper.FetchOpenAIModelsAsync(openAiKey);

            if (models.Count == 0)
            {
                Console.WriteLine("⚠️ No models retrieved (possibly offline or missing API key).");
                return;
            }

            Console.WriteLine($"✅ Retrieved {models.Count} models:");
            foreach (var m in models)
                Console.WriteLine($"  - {m.Provider}: {m.Id} ({m.MaxTokens} tokens)");

            Console.WriteLine("\nTo save these models locally, run:");
            Console.WriteLine("  tokenkit update-models [--openai-key sk-xxxx]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to scrape models: {ex.Message}");
        }
    }
}
