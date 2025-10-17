namespace TokenKit.Services.Encoders;

public interface ITextEncoder
{
    string Name { get; }
    int CountTokens(string text);
}
