using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TokenKit.Core.Extensions;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;
using TokenKit.Services.Encoders;
using TokenKit.Services;

namespace TokenKit.CLI;

[ExcludeFromCodeCoverage]
public static class TokenKitCLI
{
    private static bool JsonMode { get; set; } = false;
    private static ITokenKitCore? Core { get; set; }

    public static async Task RunAsync(string[] args)
    {
        // Bootstraps Core internally for CLI use
        var services = new ServiceCollection();
        services.AddTokenKitCore(jsonPath: Path.Combine(AppContext.BaseDirectory, "Registry", "models.data.json"));
        services.AddSingleton<ITokenizerEngine, SimpleTextEncoder>();
        services.AddSingleton<ITokenizerEngine, SharpTokenEncoder>();
        services.AddSingleton<ITokenizerEngine, MLTokenizersEncoder>();
        var provider = services.BuildServiceProvider();
        Core = provider.GetRequiredService<ITokenKitCore>();

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
            case "models":
                await HandleModelsCommandAsync(args);
                break;
            case "update-models":
                await UpdateModelsAsync(args);
                break;
            case "scrape-models":
                await ScrapeModelsAsync(args);
                break;
            case "--help":
            case "-h":
                ShowHelp();
                break;
            case "--version":
            case "-v":
                ShowVersion();
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
    private static void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var version = typeof(TokenKitCLI).Assembly.GetName().Version?.ToString() ?? "unknown";
        Console.WriteLine($"🧠 TokenKit CLI v{version}");
        Console.ResetColor();

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
    }

    private static void ShowVersion()
    {
        var version = typeof(TokenKitCLI).Assembly.GetName().Version?.ToString() ?? "unknown";
        Logger.Info($"TokenKit CLI version {version}");
        ConsoleStyler.WriteInfo($"TokenKit CLI version {version}");
    }

    // ------------------------------------------------------------
    // ANALYZE
    // ------------------------------------------------------------
    private static async Task AnalyzeAsync(string[] args)
    {
        try
        {
            var (text, modelId, engine) = await ParseInputArgsAsync(args);
            var response = await Core!.AnalyzeAsync(new AnalyzeRequest
            {
                Text = text,
                ModelId = modelId,
                Engine = engine
            });

            if (JsonMode)
            {
                Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                ConsoleStyler.WriteJson(response, success: response.Valid);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Analyze failed: {ex.Message}");
            ConsoleStyler.WriteError($"Analyze failed: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // VALIDATE
    // ------------------------------------------------------------
    private static async Task ValidateAsync(string[] args)
    {
        try
        {
            var (text, modelId, engine) = await ParseInputArgsAsync(args);
            var response = await Core!.ValidateAsync(new ValidateRequest
            {
                Text = text,
                ModelId = modelId,
                Engine = engine
            });

            if (JsonMode)
            {
                Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                ConsoleStyler.WriteJson(response, success: response.IsValid);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Validation failed: {ex.Message}");
            ConsoleStyler.WriteError($"Validation failed: {ex.Message}");
        }
    }

    // ------------------------------------------------------------
    // MODELS LIST
    // ------------------------------------------------------------
    private static async Task HandleModelsCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleStyler.WriteWarning("Missing subcommand for 'models'. Use 'tokenkit models list'.");
            return;
        }

        var sub = args[1].ToLowerInvariant();
        if (sub != "list")
        {
            ConsoleStyler.WriteWarning($"Unknown subcommand '{sub}' for 'models'.");
            return;
        }

        var providerIndex = Array.IndexOf(args, "--provider");
        string? provider = providerIndex >= 0 && providerIndex + 1 < args.Length ? args[providerIndex + 1] : null;

        var models = await Core!.GetModelsAsync(provider);

        if (JsonMode)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(models, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        if (models.Count == 0)
        {
            ConsoleStyler.WriteWarning("No models found.");
            return;
        }

        ConsoleStyler.WriteInfo($"📦 Registered Models ({models.Count}):");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine($"{"Provider",-12} {"Model ID",-22} {"MaxTokens",10} {"Input/Output (per 1K)",30}");
        Console.WriteLine("-------------------------------------------------------------");

        foreach (var m in models)
            Console.WriteLine($"{m.Provider,-12} {m.Id,-22} {m.MaxTokens,10:N0}  ${m.InputPricePer1K:F4}/${m.OutputPricePer1K:F4}");

        Console.WriteLine("-------------------------------------------------------------");
        ConsoleStyler.WriteSuccess($"Total: {models.Count} models\n");
    }


    // ------------------------------------------------------------
    // UPDATE & SCRAPE
    // ------------------------------------------------------------
    private static async Task UpdateModelsAsync(string[] args)
    {
        string? openAiKey = GetFlagValue(args, "--openai-key");
        var updater = new ModelDataUpdater("Registry/models.data.json");
        await updater.UpdateAsync(openAiKey);
        ConsoleStyler.WriteSuccess("✅ Model data updated successfully.");
    }

    private static async Task ScrapeModelsAsync(string[] args)
    {
        string? openAiKey = GetFlagValue(args, "--openai-key");
        var scraper = new ModelDataScraper();
        var models = await scraper.FetchOpenAIModelsAsync(openAiKey);

        if (JsonMode)
        {
            Console.WriteLine(JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            ConsoleStyler.WriteSuccess($"Retrieved {models.Count} models:");
            foreach (var m in models)
                ConsoleStyler.WriteInfo($"  - {m.Provider}: {m.Id} ({m.MaxTokens} tokens)");
        }
    }

    // ------------------------------------------------------------
    // UTILITIES
    // ------------------------------------------------------------
    private static async Task<(string Text, string ModelId, string Engine)> ParseInputArgsAsync(string[] args)
    {
        string? pipedInput = null;
        if (Console.IsInputRedirected)
        {
            pipedInput = await Console.In.ReadToEndAsync();
            pipedInput = pipedInput.Trim();
        }

        var modelId = GetFlagValue(args, "--model") ?? "gpt-4o";
        var engine = GetFlagValue(args, "--engine") ?? "simple";

        string text;
        if (!string.IsNullOrWhiteSpace(pipedInput))
        {
            text = pipedInput;
        }
        else if (args.Length > 1)
        {
            var inputParts = args.Skip(1).TakeWhile(a => !a.StartsWith("--")).ToArray();
            var inputArg = string.Join(" ", inputParts);
            text = File.Exists(inputArg) ? await File.ReadAllTextAsync(inputArg) : inputArg;
        }
        else
        {
            throw new InvalidOperationException("No text or file input provided.");
        }

        return (text, modelId, engine);
    }

    private static string? GetFlagValue(string[] args, string flag)
    {
        var index = Array.IndexOf(args, flag);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
