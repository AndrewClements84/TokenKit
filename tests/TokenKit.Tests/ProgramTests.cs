using System.Threading.Tasks;
using TokenKit.CLI;

namespace TokenKit.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task Program_Should_Run_WithoutError()
        {
            // Redirect output just to ensure nothing crashes
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);

            await TokenKitCLI.RunAsync(Array.Empty<string>());

            Console.SetOut(original); // restore before disposing
            sw.Dispose();

            var output = sw.ToString();
            // Just ensure something was written and no exception thrown
            Assert.False(string.IsNullOrWhiteSpace(output));
        }
    }
}

