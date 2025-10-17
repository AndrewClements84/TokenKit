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

    [Fact]
    public void Simple_And_SharpToken_Encoders_Should_Produce_Different_TokenCounts()
    {
        var text = "Hello world from TokenKit — testing tokenization!";
        var simple = new TokenizerService("simple").Analyze(text, "gpt-4o");
        var sharp = new TokenizerService("sharptoken").Analyze(text, "gpt-4o");

        Assert.NotEqual(simple.TokenCount, sharp.TokenCount);
        Assert.Equal("SimpleTokenizer", simple.Engine);
        Assert.Equal("SharpToken", sharp.Engine);
    }

    [Fact]
    public void SharpToken_And_MLTokenizers_Encoders_Should_Work_Correctly()
    {
        var text = "TokenKit makes multi-encoder testing straightforward!";
        var sharp = new TokenizerService("sharptoken").Analyze(text, "gpt-4o");
        var ml = new TokenizerService("mltokenizers").Analyze(text, "gpt-4o");

        Assert.True(sharp.TokenCount > 0);
        Assert.True(ml.TokenCount > 0);
        Assert.Equal("SharpToken", sharp.Engine);
        Assert.Equal("Microsoft.ML.Tokenizers (Tiktoken)", ml.Engine);
    }

    [Fact]
    public void Invalid_Engine_Should_Fallback_To_Simple()
    {
        var text = "Invalid engine test input";
        var invalid = new TokenizerService("unknownengine").Analyze(text, "gpt-4o");

        Assert.Equal("SimpleTokenizer", invalid.Engine);
        Assert.True(invalid.TokenCount > 0);
    }
}

