using TokenKit.Services.Encoders;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Tests.Services.Encoders
{
    public class MLTokenizersEncoderTests
    {
        private readonly ITokenizerEngine _encoder = new MLTokenizersEncoder();
        private readonly ModelInfo _model = new() { Id = "gpt-4o", Encoding = "cl100k_base" };

        [Fact]
        public void CountTokens_ShouldReturnZero_ForNullOrWhitespace()
        {
            Assert.Equal(0, _encoder.CountTokens("", _model));
            Assert.Equal(0, _encoder.CountTokens("   ", _model));
        }

        [Fact]
        public void CountTokens_ShouldReturnPositive_ForNormalText()
        {
            var count = _encoder.CountTokens("Hello TokenKit!", _model);
            Assert.True(count > 0);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            Assert.Equal("mltokenizers", _encoder.Name);
        }

        [Fact]
        public void CountTokens_ShouldUseFallback_WhenModelIdIsInvalid()
        {
            var encoder = new MLTokenizersEncoder();
            var model = new ModelInfo
            {
                Id = "non-existent-model-id", // ✅ triggers the catch block
                Encoding = "cl100k_base"
            };

            var count = encoder.CountTokens("Hello TokenKit!", model);

            // We just care that it didn’t throw and returned a positive token count.
            Assert.True(count > 0);
        }
    }
}
