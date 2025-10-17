using TokenKit.Registry;
using TokenKit.Services;

public class TokenizerTests
{
    [Fact]
    public void Analyze_Should_Return_Correct_TokenCount()
    {
        var service = new TokenizerService();
        var result = service.Analyze("Hello world from TokenKit", "gpt-4o");

        Assert.Equal(4, result.TokenCount);
        Assert.Equal("SimpleTokenizer", result.Engine); // since default encoder
    }

    [Fact]
    public void CostEstimator_Should_Return_Correct_Cost()
    {
        var model = ModelRegistry.Get("gpt-4o")!;
        var cost = CostEstimator.Estimate(model, 2000);
        Assert.True(cost > 0);
    }
}
