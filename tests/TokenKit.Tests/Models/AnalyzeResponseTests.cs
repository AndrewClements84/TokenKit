using TokenKit.Core.Models;

namespace TokenKit.Tests.Core.Models
{
    public class AnalyzeResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeDefaults()
        {
            var result = new AnalyzeResponse();

            Assert.Equal(string.Empty, result.Model);
            Assert.Equal(string.Empty, result.Provider);
            Assert.Equal("simple", result.Engine);
            Assert.Equal(0, result.TokenCount);
            Assert.Equal(0m, result.EstimatedCost);
            Assert.True(result.Valid);
            Assert.Null(result.Message);
        }

        [Fact]
        public void Properties_ShouldSetAndReturnValues()
        {
            var result = new AnalyzeResponse
            {
                Model = "gpt-4o",
                Provider = "OpenAI",
                Engine = "sharptoken",
                TokenCount = 42,
                EstimatedCost = 0.123m,
                Valid = false,
                Message = "Too long"
            };

            Assert.Equal("gpt-4o", result.Model);
            Assert.Equal("OpenAI", result.Provider);
            Assert.Equal("sharptoken", result.Engine);
            Assert.Equal(42, result.TokenCount);
            Assert.Equal(0.123m, result.EstimatedCost);
            Assert.False(result.Valid);
            Assert.Equal("Too long", result.Message);
        }

        [Fact]
        public void ShouldSerializeAndDeserializeCorrectly()
        {
            var original = new AnalyzeResponse
            {
                Model = "gpt-4o-mini",
                Provider = "OpenAI",
                Engine = "mltokenizers",
                TokenCount = 100,
                EstimatedCost = 0.005m,
                Valid = true,
                Message = "OK"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(original);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<AnalyzeResponse>(json)!;

            Assert.Equal(original.Model, deserialized.Model);
            Assert.Equal(original.Provider, deserialized.Provider);
            Assert.Equal(original.Engine, deserialized.Engine);
            Assert.Equal(original.TokenCount, deserialized.TokenCount);
            Assert.Equal(original.EstimatedCost, deserialized.EstimatedCost);
            Assert.Equal(original.Valid, deserialized.Valid);
            Assert.Equal(original.Message, deserialized.Message);
        }
    }
}

