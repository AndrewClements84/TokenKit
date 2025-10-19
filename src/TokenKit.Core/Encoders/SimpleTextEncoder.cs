using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Services.Encoders;

public class SimpleTextEncoder : ITokenizerEngine
{
    public string Name => "simple";

    public int CountTokens(string text, ModelInfo model)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

