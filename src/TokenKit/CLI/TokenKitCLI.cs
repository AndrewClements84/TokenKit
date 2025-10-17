using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TokenKit.Models;
using TokenKit.Registry;
using TokenKit.Services;

namespace TokenKit.CLI;

[ExcludeFromCodeCoverage]
public static class TokenKitCLI
{
    // Global flags so subcommands can access them
    private static bool JsonMode { get; set; } = false;

    public static async Task RunAsync(string[] args)
    {
        JsonMode = args.Contains("--json", StringComparer.OrdinalIgnoreCase);
        Logger.QuietMode = args.Contains("--quiet", StringComparer.OrdinalIgnoreCase) || JsonMode;

        if (!Logger.QuietMode)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
                ████████╗░█████╗░██╗░░██╗███████╗███╗░░██╗██╗░░██╗██╗████████╗
                ╚══██╔══╝██╔══██╗██║░██╔╝██╔════╝████╗░██║██║░██╔╝██║╚══██╔══╝
                ░░░██║░░░██║░░██║█████═╝░█████╗░░██╔██╗██║█████═╝░██║░░░██║░░░
                ░░░██║░░░██║░░██║██╔═██╗░██╔══╝░░██║╚████║██╔═██╗░██║░░░██║░░░
                ░░░██║░░░╚█████╔╝██║░╚██╗███████╗██║░╚███║██║░╚██╗██║░░░██║░░░
                ░░░╚═╝░░░░╚════╝░╚═╝░░╚═╝╚══════╝╚═╝░░╚══╝╚═╝░░╚═╝╚═╝░░░╚═╝░░░
                ");
                            Console.ResetColor();

            var version = typeof(TokenKitCLI).Assembly.GetName().Version?.ToString() ?? "unknown";
            ConsoleStyler.WriteInfo($"TokenKit v{version}  |  AndrewClements84 © 2025 |  https://github.com/AndrewClements84/TokenKit\n");
        }
        else if (!JsonMode)
        {
            ConsoleStyler.WriteInfo("(quiet mode active — output will only be written to tokenkit.log)");
        }

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
            case "--version":
            case "-v":
                ShowVersion();
                break;
            case "--help":
            case "-h":
                var compact = args.Length > 1 && args[1].Equals("short", StringComparison.OrdinalIgnoreCase);
                ShowHelp(compact);
                break;
            default:
                Logger.Warn($"Unknown command: {command}");
                ConsoleStyler.WriteWarning("Unknown command. Use '--help' to see available commands.");
                break;
        }
    }

    // ------------------------------------------------------------
    // HELP & VERSION
    // ------------------------------------------------------------

    private static void ShowHelp(bool compact = false)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var version = typeof(TokenKitCLI).Assembly.GetName().Version?.ToString() ?? "unknown";
        Console.WriteLine($"🧠 TokenKit CLI v{version}");
        Console.ResetColor();

        if (compact)
        {
            Console.WriteLine("Usage: tokenkit <command> [options]");
            Console.WriteLine("Try 'tokenkit --help' for more details.");
            return;
        }

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("Tokenization & Validation Toolkit for LLMs");
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine();

        ConsoleStyler.WriteInfo("📘 Usage:");
        Console.WriteLine("  tokenkit analyze \"<text|file>\" --model <model-id> [--engine <engine>]");
        Console.WriteLine("  tokenkit validate \"<text|file>\" --model <model-id> [--engine <engine>]");
        Console.WriteLine("  tokenkit models list [--provider <name>]");
        Console.WriteLine("  tokenkit update-models [--openai-key <key>]");
        Console.WriteLine("  tokenkit scrape-models [--openai-key <key>]");
        Console.WriteLine();

        ConsoleStyler.WriteInfo("⚙️ Options:");
        Console.WriteLine("  --engine <engine>     Select encoding engine [simple|sharptoken|mltokenizers]");
        Console.WriteLine("  --model <model-id>    Specify model to analyze/validate (e.g., gpt-4o)");
        Console.WriteLine("  --openai-key <key>    Provide OpenAI API key for live updates");
        Console.WriteLine("  --provider <name>     Filter models by provider (case-insensitive)");
        Console.WriteLine("  --json                Output results as raw JSON (suppresses colored output)");
        Console.WriteLine("  --quiet               Suppress console output (logs still written to tokenkit.log)");
        Console.WriteLine("  --version, -v         Display the installed TokenKit version");
        Console.WriteLine("  --help, -h            Show this help message");
        Console.WriteLine();

        ConsoleStyler.WriteInfo("🔍 Examples:");
        Console.WriteLine("  tokenkit analyze \"Hello from TokenKit!\" --model gpt-4o --engine sharptoken");
        Console.WriteLine("  tokenkit validate prompt.txt --model gpt-4o");
        Console.WriteLine("  tokenkit models list --provider Anthropic");
        Console.WriteLine("  tokenkit analyze \"Hello\" --model gpt-4o --json");
        Console.WriteLine();

        ConsoleStyler.WriteInfo("🧾 Output:");
        Console.WriteLine("  JSON summary including token count, cost, and validation status.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("© 2025 AndrewClements84 | MIT License | https://github.com/AndrewClements84/TokenKit");
        Console.WriteLine("------------------------------------------------------------");
        Console.ResetColor();
    }

    private static void ShowVersion()
    {
        var version = typeof(TokenKitCLI).Assembly.GetName().Version?.ToString() ?? "unknown";
        Logger.Info($"TokenKit CLI version {version}");
        ConsoleStyler.WriteInfo($"TokenKit CLI version {version}");
    }

    // ------------------------------------------------------------
    // MODELS COMMAND
    // ------------------------------------------------------------
    private static async Task HandleModelsCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Logger.Error("Missing subcommand for 'models'.");
            ConsoleStyler.WriteError("Missing subcommand. Use: tokenkit models list");
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
                Logger.Warn($"Unknown models subcommand: {subcommand}");
                ConsoleStyler.WriteWarning($"Unknown subcommand '{subcommand}'. Try: tokenkit models list");
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
                Logger.Warn("No models found in registry.");
                ConsoleStyler.WriteWarning("No models found in registry.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(providerFilter))
            {
                models = models
                    .Where(m => m.Provider.Equals(providerFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (models.Count == 0)
                {
                    Logger.Warn($"No models found for provider '{providerFilter}'.");
                    ConsoleStyler.WriteWarning($"No models found for provider '{providerFilter}'.");
                    return;
                }
            }

            if (JsonMode)
            {
                var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
                return;
            }

            Logger.Info($"Listing {models.Count} models {(providerFilter != null ? $"for {providerFilter}" : "")}");
            ConsoleStyler.WriteInfo("📦 Registered Models:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"{"Provider",-12} {"Model ID",-22} {"MaxTokens",10} {"Input/Output (per 1K)",30}");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var m in models)
            {
                Console.WriteLine($"{m.Provider,-12} {m.Id,-22} {m.MaxTokens,10:N0}  " +
                    $"${m.InputPricePer1K:F4}/${m.OutputPricePer1K:F4}");
            }

            Console.WriteLine("-------------------------------------------------------------");
            Logger.Success($"Listed {models.Count} models");
            ConsoleStyler.WriteSuccess($"Total: {models.Count} {(providerFilter != null ? $"({providerFilter})" : "")} models\n");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to list models: {ex.Message}");
            ConsoleStyler.WriteError($"Failed to list models: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // ANALYZE COMMAND
    // ------------------------------------------------------------
    private static async Task AnalyzeAsync(string[] args)
    {
        try
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

            var engineFlagIndex = Array.IndexOf(args, "--engine");
            var engineName = (engineFlagIndex >= 0 && engineFlagIndex + 1 < args.Length)
                ? args[engineFlagIndex + 1]
                : "simple";

            Logger.Info($"Analyze started with model={modelId}, engine={engineName}");

            string text;

            if (!string.IsNullOrWhiteSpace(pipedInput))
                text = pipedInput;
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
                Logger.Error("No input provided to analyze.");
                ConsoleStyler.WriteError("Missing text, file path, or stdin input.");
                return;
            }

            var tokenizer = new TokenizerService(engineName);
            var result = tokenizer.Analyze(text, modelId);

            var model = ModelRegistry.Get(modelId);
            if (model == null)
            {
                Logger.Error($"Model '{modelId}' not found in registry.");
                ConsoleStyler.WriteError($"Model '{modelId}' not found in registry.");
                return;
            }

            var cost = CostEstimator.Estimate(model, result.TokenCount);
            var valid = result.TokenCount <= model.MaxTokens;

            var output = new
            {
                Model = model.Id,
                Provider = model.Provider,
                result.TokenCount,
                EstimatedCost = cost,
                Engine = result.Engine,
                Valid = valid
            };

            Logger.Success($"Analyzed {result.TokenCount} tokens using {engineName} ({modelId})");

            if (JsonMode)
            {
                var json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }
            else
            {
                ConsoleStyler.WriteJson(output, success: valid);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Analyze failed: {ex.Message}");
            ConsoleStyler.WriteError($"Analyze failed: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // VALIDATE COMMAND
    // ------------------------------------------------------------
    private static async Task ValidateAsync(string[] args)
    {
        try
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

            var engineFlagIndex = Array.IndexOf(args, "--engine");
            var engineName = (engineFlagIndex >= 0 && engineFlagIndex + 1 < args.Length)
                ? args[engineFlagIndex + 1]
                : "simple";

            Logger.Info($"Validation started for {modelId} with engine={engineName}");

            string text;

            if (!string.IsNullOrWhiteSpace(pipedInput))
                text = pipedInput;
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
                Logger.Error("No input provided to validate.");
                ConsoleStyler.WriteError("Missing text, file path, or stdin input.");
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

            Logger.Info($"Validated {result.TokenCount} tokens ({validation.Message})");

            if (JsonMode)
            {
                var json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }
            else
            {
                ConsoleStyler.WriteJson(output, success: validation.IsValid);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Validation failed: {ex.Message}");
            ConsoleStyler.WriteError($"Validation failed: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // UPDATE-MODELS COMMAND
    // ------------------------------------------------------------
    private static async Task UpdateModelsAsync(string[] args)
    {
        try
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
                        var jsonOut = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync("Registry/models.data.json", jsonOut);
                        Logger.Success($"Updated model registry with {models.Count} entries from stdin.");

                        if (JsonMode)
                        {
                            Console.WriteLine(JsonSerializer.Serialize(
                                new { Updated = models.Count, Source = "stdin" },
                                new JsonSerializerOptions { WriteIndented = true }));
                        }
                        else
                        {
                            ConsoleStyler.WriteSuccess($"Updated model registry with {models.Count} entries from stdin.");
                        }
                        return;
                    }

                    Logger.Warn("No valid model data found in stdin input.");
                    if (!JsonMode) ConsoleStyler.WriteWarning("No valid model data found in stdin input.");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to parse JSON input: {ex.Message}");
                    if (!JsonMode) ConsoleStyler.WriteError($"Failed to parse JSON input: {ex.Message}");
                }
            }
            else
            {
                await updater.UpdateAsync(openAiKey);
                Logger.Success("Model data updated successfully from available source.");

                if (JsonMode)
                {
                    Console.WriteLine(JsonSerializer.Serialize(
                        new { Updated = true, Source = openAiKey is null ? "fallback" : "openai" },
                        new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    ConsoleStyler.WriteSuccess("Model data updated successfully from available source.");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"UpdateModels failed: {ex.Message}");
            if (!JsonMode) ConsoleStyler.WriteError($"UpdateModels failed: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // SCRAPE-MODELS COMMAND
    // ------------------------------------------------------------
    private static async Task ScrapeModelsAsync(string[] args)
    {
        Logger.Info("Fetching latest OpenAI model data...");
        if (!JsonMode) ConsoleStyler.WriteInfo("🔍 Fetching latest OpenAI model data...");

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
                Logger.Warn("No models retrieved (offline or missing API key).");
                if (JsonMode)
                {
                    Console.WriteLine(JsonSerializer.Serialize(
                        new { Retrieved = 0 },
                        new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    ConsoleStyler.WriteWarning("No models retrieved (possibly offline or missing API key).");
                }
                return;
            }

            Logger.Success($"Retrieved {models.Count} models.");

            if (JsonMode)
            {
                Console.WriteLine(JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                ConsoleStyler.WriteSuccess($"Retrieved {models.Count} models:");
                foreach (var m in models)
                    ConsoleStyler.WriteInfo($"  - {m.Provider}: {m.Id} ({m.MaxTokens} tokens)");

                Console.WriteLine();
                ConsoleStyler.WriteInfo("To save these models locally, run:");
                Console.WriteLine("  tokenkit update-models [--openai-key sk-xxxx]");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"ScrapeModels failed: {ex.Message}");
            if (!JsonMode) ConsoleStyler.WriteError($"Failed to scrape models: {ex.Message}");
        }
    }
}
