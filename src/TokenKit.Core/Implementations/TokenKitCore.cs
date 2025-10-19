using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;
using TokenKit.Core.Encoders; // <-- NEW: add this to access TokenKitCoreEncoders

namespace TokenKit.Core.Implementations;

public sealed class TokenKitCore : ITokenKitCore
{
    private readonly IModelRegistry _registry;
    private readonly IDictionary<string, ITokenizerEngine> _engines;
    private readonly ICostEstimator _costs;

    // ✅ Static constructor ensures encoders are registered once at startup
    static TokenKitCore()
    {
        TokenKitCoreEncoders.RegisterDefaults();
    }

    public TokenKitCore(IModelRegistry registry, IEnumerable<ITokenizerEngine> engines, ICostEstimator costs)
    {
        _registry = registry;
        _engines = engines.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

        // ✅ Defensive fallback: if no encoders injected, use the registered defaults
        if (_engines.Count == 0 && TokenKitCoreEncoders.Registered.Any())
        {
            _engines = TokenKitCoreEncoders.Registered
                .ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);
        }

        _costs = costs;
    }

    public Task<IReadOnlyList<ModelInfo>> GetModelsAsync(string? provider = null, CancellationToken ct = default)
        => Task.FromResult(_registry.GetAll(provider));

    public Task<AnalyzeResponse> AnalyzeAsync(AnalyzeRequest req, CancellationToken ct = default)
    {
        var model = _registry.Get(req.ModelId) ?? throw new ArgumentException($"Unknown model '{req.ModelId}'.");
        var engine = ResolveEngine(req.Engine);

        var count = engine.CountTokens(req.Text, model);
        var cost = _costs.EstimateTotal(model, count);
        var valid = count <= model.MaxTokens;

        return Task.FromResult(new AnalyzeResponse
        {
            Model = model.Id,
            Provider = model.Provider,
            Engine = engine.Name,
            TokenCount = count,
            EstimatedCost = cost,
            Valid = valid,
            Message = valid ? "OK" : $"Exceeds MaxTokens ({model.MaxTokens})."
        });
    }

    public async Task<ValidateResponse> ValidateAsync(ValidateRequest req, CancellationToken ct = default)
    {
        var a = await AnalyzeAsync(new AnalyzeRequest
        {
            Text = req.Text,
            ModelId = req.ModelId,
            Engine = req.Engine
        }, ct);

        return new ValidateResponse
        {
            IsValid = a.Valid,
            Message = a.Message,
            TokenCount = a.TokenCount,
            MaxTokens = _registry.Get(req.ModelId)!.MaxTokens
        };
    }

    private ITokenizerEngine ResolveEngine(string? requested)
    {
        // ✅ Try requested first
        if (!string.IsNullOrWhiteSpace(requested) && _engines.TryGetValue(requested, out var eng))
            return eng;

        // ✅ Otherwise fallback to first registered default encoder
        if (_engines.Count > 0)
            return _engines.Values.First();

        // ✅ Last resort: if encoders somehow never registered, throw clear message
        throw new InvalidOperationException("No tokenizer engines registered in TokenKitCore.");
    }
}
