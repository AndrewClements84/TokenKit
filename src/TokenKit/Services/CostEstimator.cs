using TokenKit.Models;

namespace TokenKit.Services;

public static class CostEstimator
{
    public static decimal Estimate(ModelSpec model, int inputTokens, int outputTokens = 0)
    {
        var inputCost = (inputTokens / 1000m) * model.InputPricePer1K;
        var outputCost = (outputTokens / 1000m) * model.OutputPricePer1K;
        return Math.Round(inputCost + outputCost, 6);
    }
}
