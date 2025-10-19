using TokenKit.Core.Models;

namespace TokenKit.Core.Interfaces;

public interface ITokenizerEngine
{
    string Name { get; }
    int CountTokens(string text, ModelInfo model);
}

