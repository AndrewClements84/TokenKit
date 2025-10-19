namespace TokenKit.Core.Models;

public sealed class AnalyzeRequest
{
    public string Text { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;  // e.g., "gpt-4o"
    public string? Engine { get; init; }                  // e.g., "simple" | "sharptoken" | "mltokenizers"
}

