using TokenKit.Core.Models;

namespace TokenKit.Core.Interfaces;

public interface ICostEstimator
{
    decimal EstimateTotal(ModelInfo model, int tokenCount);
}

