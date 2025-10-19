namespace TokenKit.Core.Models;

public sealed class ModelInfo
{
    public string Id { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public int MaxTokens { get; init; }
    public decimal InputPricePer1K { get; init; }
    public decimal OutputPricePer1K { get; init; }
    public string Encoding { get; init; } = string.Empty;
}
