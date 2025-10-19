using TokenKit.Services.Encoders;
using TokenKit.Core.Models;
using TokenKit.Core.Interfaces;

namespace TokenKit.Tests.Services.Encoders
{
    public class SimpleTextEncoderTests
    {
        private readonly ITokenizerEngine _encoder = new SimpleTextEncoder();
        private readonly ModelInfo _model = new() { Id = "test-model", Encoding = "none" };

        [Fact]
        public void CountTokens_ShouldReturnZero_ForNullOrWhitespace()
        {
            Assert.Equal(0, _encoder.CountTokens("", _model));
            Assert.Equal(0, _encoder.CountTokens("   ", _model));
            Assert.Equal(0, _encoder.CountTokens("\t\n", _model));
        }

        [Fact]
        public void CountTokens_ShouldReturnCorrectCount_ForNormalText()
        {
            var count = _encoder.CountTokens("Hello world from TokenKit", _model);
            Assert.Equal(4, count);
        }

        [Fact]
        public void CountTokens_ShouldIgnoreExtraWhitespace()
        {
            var count = _encoder.CountTokens("  Hello   world   ", _model);
            Assert.Equal(2, count);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            Assert.Equal("simple", _encoder.Name);
        }
    }
}
