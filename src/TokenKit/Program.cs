using System.Diagnostics.CodeAnalysis;
using TokenKit.CLI;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        await TokenKitCLI.RunAsync(args);
    }
}

