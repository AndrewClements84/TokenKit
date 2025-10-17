using TokenKit.Models;

namespace TokenKit.Tests.Models
{
    public class TokenizationResultTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeDefaults()
        {
            var result = new TokenizationResult();

            Assert.Equal(string.Empty, result.ModelId);
            Assert.Equal(0, result.TokenCount);
            Assert.NotNull(result.Tokens);
            Assert.Empty(result.Tokens);
        }

        [Fact]
        public void Properties_ShouldSetAndReturnValues()
        {
            var tokens = new List<string> { "hello", "world" };
            var result = new TokenizationResult
            {
                ModelId = "gpt-4o",
                TokenCount = 2,
                Tokens = tokens
            };

            Assert.Equal("gpt-4o", result.ModelId);
            Assert.Equal(2, result.TokenCount);
            Assert.Equal(tokens, result.Tokens);
        }

        [Fact]
        public void Tokens_ShouldBeImmutable_WhenSetAsReadOnlyList()
        {
            var result = new TokenizationResult
            {
                Tokens = new List<string> { "a", "b", "c" }
            };

            var readOnly = result.Tokens;
            Assert.IsAssignableFrom<IReadOnlyList<string>>(readOnly);
            Assert.Equal(3, readOnly.Count);
            Assert.Equal("a", readOnly[0]);
        }
    }
}
