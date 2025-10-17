using TokenKit.Models;

namespace TokenKit.Tests.Models
{
    public class ValidationResultTests
    {
        [Fact]
        public void Valid_ShouldReturnTrueWithOkMessage()
        {
            var result = ValidationResult.Valid();

            Assert.True(result.IsValid);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public void Invalid_ShouldReturnFalseWithProvidedMessage()
        {
            var result = ValidationResult.Invalid("Prompt too long");

            Assert.False(result.IsValid);
            Assert.Equal("Prompt too long", result.Message);
        }

        [Fact]
        public void RecordEquality_ShouldWorkAsExpected()
        {
            var r1 = new ValidationResult(true, "OK");
            var r2 = new ValidationResult(true, "OK");
            var r3 = new ValidationResult(false, "Error");

            Assert.Equal(r1, r2);
            Assert.NotEqual(r1, r3);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
        }

        [Fact]
        public void Deconstruct_ShouldReturnExpectedValues()
        {
            var result = new ValidationResult(true, "OK");
            var (isValid, message) = result;

            Assert.True(isValid);
            Assert.Equal("OK", message);
        }
    }
}

