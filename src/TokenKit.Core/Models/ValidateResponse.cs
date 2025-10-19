namespace TokenKit.Core.Models;

public sealed class ValidateResponse
{
    public bool IsValid { get; init; }
    public string? Message { get; init; }
    public int TokenCount { get; init; }
    public int MaxTokens { get; init; }
}
