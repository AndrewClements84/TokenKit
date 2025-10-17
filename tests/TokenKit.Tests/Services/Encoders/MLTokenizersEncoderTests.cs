using TokenKit.Services.Encoders;

namespace TokenKit.Tests.Services.Encoders
{
    public class MLTokenizersEncoderTests
    {
        [Fact]
        public void CountTokens_ShouldReturnZero_ForNullOrWhitespace()
        {
            var encoder = new MLTokenizersEncoder("gpt-4o");

            Assert.Equal(0, encoder.CountTokens(null));
            Assert.Equal(0, encoder.CountTokens(string.Empty));
            Assert.Equal(0, encoder.CountTokens("   "));
        }

        [Fact]
        public void CountTokens_ShouldReturnPositive_ForNormalText()
        {
            var encoder = new MLTokenizersEncoder("gpt-4o");

            var count = encoder.CountTokens("Hello TokenKit!");
            Assert.True(count > 0);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            var encoder = new MLTokenizersEncoder("gpt-4o");
            Assert.Equal("Microsoft.ML.Tokenizers (Tiktoken)", encoder.Name);
        }
    }
}
