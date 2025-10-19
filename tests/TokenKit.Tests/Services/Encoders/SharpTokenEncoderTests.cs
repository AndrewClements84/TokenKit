using TokenKit.Services.Encoders;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Tests.Services.Encoders
{
    public class SharpTokenEncoderTests
    {
        private readonly ITokenizerEngine _encoder = new SharpTokenEncoder();
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
            var count = _encoder.CountTokens("Hello from TokenKit!", _model);
            Assert.True(count > 0);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            Assert.Equal("sharptoken", _encoder.Name);
        }

        [Fact]
        public void CountTokens_ShouldUseDefaultEncoding_WhenModelEncodingIsNullOrEmpty()
        {
            // Arrange
            var encoder = new SharpTokenEncoder();
            var model = new ModelInfo
            {
                Id = "gpt-4o",
                Encoding = null // ✅ forces fallback to "cl100k_base"
            };

            // Act
            var count = encoder.CountTokens("Hello default encoding!", model);

            // Assert
            Assert.True(count > 0);
        }

        [Fact]
        public void CountTokens_ShouldUseDefaultEncoding_WhenModelEncodingIsWhitespace()
        {
            var encoder = new SharpTokenEncoder();
            var model = new ModelInfo { Id = "gpt-4o", Encoding = "   " }; // whitespace

            var count = encoder.CountTokens("Hello TokenKit coverage!", model);

            Assert.True(count > 0);
        }

    }
}
