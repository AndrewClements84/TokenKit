namespace TokenKit.Services.Encoders;

public class SimpleTextEncoder : ITextEncoder
{
    public string Name => "SimpleTokenizer";

    public int CountTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

