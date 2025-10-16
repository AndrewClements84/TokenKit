using TokenKit.Models;

namespace TokenKit.Services;

public class TokenizerService
{
    public TokenizationResult Analyze(string input, string modelId)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new TokenizationResult
        {
            ModelId = modelId,
            TokenCount = tokens.Length,
            Tokens = tokens
        };
    }
}

