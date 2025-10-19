using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Core.Implementations;

public sealed class SimpleTokenizerEngine : ITokenizerEngine
{
    public string Name => "simple";

    public int CountTokens(string text, ModelInfo model)
        => string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split((char[])[' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
}

