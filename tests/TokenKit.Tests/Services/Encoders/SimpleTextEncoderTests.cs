using TokenKit.Services.Encoders;

namespace TokenKit.Tests.Services.Encoders
{
    public class SimpleTextEncoderTests
    {
        [Fact]
        public void CountTokens_ShouldReturnZero_ForNullOrWhitespace()
        {
            var encoder = new SimpleTextEncoder();

            Assert.Equal(0, encoder.CountTokens(null));
            Assert.Equal(0, encoder.CountTokens(string.Empty));
            Assert.Equal(0, encoder.CountTokens("   "));
            Assert.Equal(0, encoder.CountTokens("\t\n"));
        }

        [Fact]
        public void CountTokens_ShouldReturnCorrectCount_ForNormalText()
        {
            var encoder = new SimpleTextEncoder();
            var count = encoder.CountTokens("Hello world from TokenKit");
            Assert.Equal(4, count);
        }

        [Fact]
        public void CountTokens_ShouldIgnoreExtraWhitespace()
        {
            var encoder = new SimpleTextEncoder();
            var count = encoder.CountTokens("  Hello   world   ");
            Assert.Equal(2, count);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            var encoder = new SimpleTextEncoder();
            Assert.Equal("SimpleTokenizer", encoder.Name);
        }
    }
}

