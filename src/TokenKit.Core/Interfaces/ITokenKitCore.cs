using TokenKit.Core.Models;

namespace TokenKit.Core.Interfaces;

public interface ITokenKitCore
{
    Task<AnalyzeResponse> AnalyzeAsync(AnalyzeRequest req, CancellationToken ct = default);
    Task<ValidateResponse> ValidateAsync(ValidateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ModelInfo>> GetModelsAsync(string? provider = null, CancellationToken ct = default);
}

