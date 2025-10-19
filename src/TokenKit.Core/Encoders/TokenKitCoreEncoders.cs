using TokenKit.Core.Interfaces;
using TokenKit.Services.Encoders;

namespace TokenKit.Core.Encoders;

public static class TokenKitCoreEncoders
{
    private static readonly List<ITokenizerEngine> _registered = new();

    public static IReadOnlyList<ITokenizerEngine> Registered => _registered;

    public static void RegisterDefaults()
    {
        if (_registered.Count > 0) return; // Avoid double-registering
        _registered.Add(new SharpTokenEncoder());
        _registered.Add(new MLTokenizersEncoder());
        _registered.Add(new SimpleTextEncoder());
    }

    public static ITokenizerEngine? Get(string name)
        => _registered.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

