using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Core.Implementations;

public sealed class BasicCostEstimator : ICostEstimator
{
    public decimal EstimateTotal(ModelInfo model, int tokenCount)
    {
        var perToken = (model.InputPricePer1K + model.OutputPricePer1K) / 1000m;
        return Math.Round(tokenCount * perToken, 6);
    }
}
