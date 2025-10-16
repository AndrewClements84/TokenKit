// Models/TokenizationResult.cs
namespace TokenKit.Models;

public class TokenizationResult
{
    public string ModelId { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public IReadOnlyList<string> Tokens { get; set; } = Array.Empty<string>();
}
