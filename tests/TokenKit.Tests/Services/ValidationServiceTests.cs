using TokenKit.Models;
using TokenKit.Services;

namespace TokenKit.Tests.Services
{
    public class ValidationServiceTests
    {
        [Fact]
        public void Validate_ShouldReturnValid_WhenWithinTokenLimit()
        {
            var model = new ModelSpec { Id = "gpt-4o", MaxTokens = 1000 };
            var service = new ValidationService();

            var result = service.Validate(model, 500);

            Assert.True(result.IsValid);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnInvalid_WhenTokenCountExceedsLimit()
        {
            var model = new ModelSpec { Id = "gpt-4o", MaxTokens = 1000 };
            var service = new ValidationService();

            var result = service.Validate(model, 1500);

            Assert.False(result.IsValid);
            Assert.Contains("exceeds 1000 tokens", result.Message);
            Assert.Contains("gpt-4o", result.Message);
        }
    }
}

