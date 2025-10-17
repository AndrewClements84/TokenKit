using Microsoft.ML.Tokenizers;

namespace TokenKit.Services.Encoders;

public class MLTokenizersEncoder : ITextEncoder
{
    private readonly Tokenizer _tokenizer;
    public string Name => "Microsoft.ML.Tokenizers (Tiktoken)";

    public MLTokenizersEncoder(string model = "gpt-4o")
    {
        _tokenizer = TiktokenTokenizer.CreateForModel(model);
    }

    public int CountTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        // EncodeToIds returns IReadOnlyList<int>
        return _tokenizer.EncodeToIds(text).Count;
    }
}

