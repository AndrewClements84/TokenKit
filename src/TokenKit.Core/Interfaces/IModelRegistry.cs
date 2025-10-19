using TokenKit.Core.Models;

namespace TokenKit.Core.Interfaces;

public interface IModelRegistry
{
    ModelInfo? Get(string id);
    IReadOnlyList<ModelInfo> GetAll(string? provider = null);
}

