using Microsoft.ML.Tokenizers;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Services.Encoders;

public class MLTokenizersEncoder : ITokenizerEngine
{
    public string Name => "mltokenizers";

    public int CountTokens(string text, ModelInfo model)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        Tokenizer tokenizer;

        // Prefer CreateForModel(model.Id) when supported
        try
        {
            tokenizer = TiktokenTokenizer.CreateForModel(model.Id);
        }
        catch
        {
            // Fallback if model.Id not recognized: try encoding, else a sensible default
            var enc = string.IsNullOrWhiteSpace(model.Encoding) ? "cl100k_base" : model.Encoding;
            tokenizer = TiktokenTokenizer.CreateForEncoding(enc);
        }

        return tokenizer.EncodeToIds(text).Count;
    }
}

