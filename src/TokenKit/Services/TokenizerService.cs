using System.Diagnostics;
using TokenKit.Services.Encoders;

namespace TokenKit.Services;

public class TokenizerService
{
    private readonly ITextEncoder _encoder;

    public TokenizerService(string engineName = "simple")
    {
        _encoder = CreateEncoder(engineName);
        Debug.WriteLine($"[TokenizerService] Using encoder: {_encoder.Name}");
    }

    private static ITextEncoder CreateEncoder(string engine)
    {
        switch (engine.ToLowerInvariant())
        {
            case "sharptoken":
                return new SharpTokenEncoder();
            case "mltokenizers":
            case "ml":
                return new MLTokenizersEncoder();
            default:
                return new SimpleTextEncoder();
        }
    }

    public (int TokenCount, string Engine) Analyze(string text, string modelId)
    {
        var count = _encoder.CountTokens(text);
        return (count, _encoder.Name);
    }
}
