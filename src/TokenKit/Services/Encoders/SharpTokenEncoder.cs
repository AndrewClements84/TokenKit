using SharpToken;

namespace TokenKit.Services.Encoders;

public class SharpTokenEncoder : ITextEncoder
{
    private readonly GptEncoding _encoding;
    public string Name => "SharpToken";

    public SharpTokenEncoder(string encodingName = "cl100k_base")
    {
        _encoding = GptEncoding.GetEncoding(encodingName);
    }

    public int CountTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return _encoding.Encode(text).Count;
    }
}


