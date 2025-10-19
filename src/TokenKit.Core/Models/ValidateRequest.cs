namespace TokenKit.Core.Models;

public sealed class ValidateRequest
{
    public string Text { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public string? Engine { get; init; }
}

