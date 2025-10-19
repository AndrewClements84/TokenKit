using TokenKit.Core.Implementations;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;

namespace TokenKit.Tests.Services
{
    public class SimpleTokenizerEngineTests
    {
        private readonly ITokenizerEngine _engine = new SimpleTokenizerEngine();
        private readonly ModelInfo _model = new() { Id = "test", Encoding = "none" };

        [Fact]
        public void CountTokens_ShouldReturnZero_WhenTextIsNullOrWhitespace()
        {
            Assert.Equal(0, _engine.CountTokens(null!, _model));   // null
            Assert.Equal(0, _engine.CountTokens("", _model));       // empty
            Assert.Equal(0, _engine.CountTokens("   ", _model));    // whitespace
            Assert.Equal(0, _engine.CountTokens("\n\r\t", _model)); // control chars
        }

        [Fact]
        public void CountTokens_ShouldReturnCorrectCount_ForSimpleText()
        {
            var text = "Hello world from TokenKit";
            var count = _engine.CountTokens(text, _model);

            Assert.Equal(4, count); // 4 words
        }

        [Fact]
        public void CountTokens_ShouldHandleMixedWhitespaceCorrectly()
        {
            var text = "Hello\tworld\nfrom   TokenKit\rrocks";
            var count = _engine.CountTokens(text, _model);

            Assert.Equal(5, count);
        }

        [Fact]
        public void Name_ShouldBeSimple()
        {
            Assert.Equal("simple", _engine.Name);
        }
    }
}
