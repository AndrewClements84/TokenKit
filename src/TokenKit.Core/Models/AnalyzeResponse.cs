namespace TokenKit.Core.Models;

public sealed class AnalyzeResponse
{
    public string Model { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Engine { get; init; } = "simple";
    public int TokenCount { get; init; }
    public decimal EstimatedCost { get; init; }
    public bool Valid { get; init; } = true;
    public string? Message { get; init; }
}
