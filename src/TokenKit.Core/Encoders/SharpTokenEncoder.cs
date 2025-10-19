using SharpToken;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Services.Encoders;

public class SharpTokenEncoder : ITokenizerEngine
{
    public string Name => "sharptoken";

    public int CountTokens(string text, ModelInfo model)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        // Prefer model.Encoding; default to cl100k_base if blank
        var encodingName = string.IsNullOrWhiteSpace(model.Encoding) ? "cl100k_base" : model.Encoding;
        var encoding = GptEncoding.GetEncoding(encodingName);
        return encoding.Encode(text).Count;
    }
}


