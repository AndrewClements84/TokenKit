using TokenKit.Core.Models;

namespace TokenKit.Tests.Core.Models
{
    public class ValidateResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeDefaults()
        {
            var result = new ValidateResponse();

            Assert.False(result.IsValid);
            Assert.Null(result.Message);
            Assert.Equal(0, result.TokenCount);
            Assert.Equal(0, result.MaxTokens);
        }

        [Fact]
        public void Properties_ShouldSetAndReturnValues()
        {
            var result = new ValidateResponse
            {
                IsValid = true,
                Message = "OK",
                TokenCount = 42,
                MaxTokens = 128000
            };

            Assert.True(result.IsValid);
            Assert.Equal("OK", result.Message);
            Assert.Equal(42, result.TokenCount);
            Assert.Equal(128000, result.MaxTokens);
        }

        [Fact]
        public void ShouldSerializeAndDeserializeCorrectly()
        {
            var original = new ValidateResponse
            {
                IsValid = false,
                Message = "Exceeded token limit",
                TokenCount = 200000,
                MaxTokens = 128000
            };

            var json = System.Text.Json.JsonSerializer.Serialize(original);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<ValidateResponse>(json)!;

            Assert.Equal(original.IsValid, deserialized.IsValid);
            Assert.Equal(original.Message, deserialized.Message);
            Assert.Equal(original.TokenCount, deserialized.TokenCount);
            Assert.Equal(original.MaxTokens, deserialized.MaxTokens);
        }

        [Fact]
        public void Equality_ShouldBeComparedByProperties()
        {
            var a = new ValidateResponse { IsValid = true, Message = "OK", TokenCount = 10, MaxTokens = 100 };
            var b = new ValidateResponse { IsValid = true, Message = "OK", TokenCount = 10, MaxTokens = 100 };
            var c = new ValidateResponse { IsValid = false, Message = "Error", TokenCount = 5, MaxTokens = 50 };

            Assert.True(AreEqual(a, b));
            Assert.False(AreEqual(a, c));
        }

        private static bool AreEqual(ValidateResponse x, ValidateResponse y)
        {
            return x.IsValid == y.IsValid &&
                   x.Message == y.Message &&
                   x.TokenCount == y.TokenCount &&
                   x.MaxTokens == y.MaxTokens;
        }
    }
}
