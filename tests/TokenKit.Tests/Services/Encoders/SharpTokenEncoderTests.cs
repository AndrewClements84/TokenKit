using TokenKit.Services.Encoders;

namespace TokenKit.Tests.Services.Encoders
{
    public class SharpTokenEncoderTests
    {
        [Fact]
        public void CountTokens_ShouldReturnZero_ForNullOrWhitespace()
        {
            var encoder = new SharpTokenEncoder("cl100k_base");

            Assert.Equal(0, encoder.CountTokens(null));
            Assert.Equal(0, encoder.CountTokens(string.Empty));
            Assert.Equal(0, encoder.CountTokens("   "));
        }

        [Fact]
        public void CountTokens_ShouldReturnPositive_ForNormalText()
        {
            var encoder = new SharpTokenEncoder("cl100k_base");
            var count = encoder.CountTokens("Hello from TokenKit!");
            Assert.True(count > 0);
        }

        [Fact]
        public void Name_ShouldBeCorrect()
        {
            var encoder = new SharpTokenEncoder();
            Assert.Equal("SharpToken", encoder.Name);
        }
    }
}

