using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TokenKit.Core.Extensions;
using TokenKit.Core.Interfaces;
using TokenKit.Services.Encoders;
using TokenKit.CLI;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Core (points to your runtime models.data.json location)
        services.AddTokenKitCore(
            jsonPath: Path.Combine(AppContext.BaseDirectory, "Registry", "models.data.json")
        );

        // Register your encoders as Core engines
        services.AddSingleton<ITokenizerEngine, SimpleTextEncoder>();
        services.AddSingleton<ITokenizerEngine, SharpTokenEncoder>();
        services.AddSingleton<ITokenizerEngine, MLTokenizersEncoder>();

        var provider = services.BuildServiceProvider();

        // If/when your CLI takes dependencies, you can pass provider or resolve ITokenKitCore here
        // var core = provider.GetRequiredService<ITokenKitCore>();

        await TokenKitCLI.RunAsync(args);
    }
}