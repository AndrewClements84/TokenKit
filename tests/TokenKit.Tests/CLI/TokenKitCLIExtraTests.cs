using System.Text.Json;
using TokenKit.CLI;
using TokenKit.Core.Models;

namespace TokenKit.Tests.CLI
{
    public class TokenKitCLIExtraTests : IDisposable
    {
        private readonly TextWriter _originalOut;

        public TokenKitCLIExtraTests()
        {
            _originalOut = Console.Out;
            Logger.QuietMode = false;
        }

        public void Dispose()
        {
            Logger.QuietMode = false;
            Console.SetOut(_originalOut);
        }

        private static string CaptureConsole(Func<Task> func)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            func().GetAwaiter().GetResult();
            Console.Out.Flush();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            return sw.ToString();
        }

        [Fact]
        public void ShowVersion_ShouldPrintVersion()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--version" }));
            Assert.Contains("version", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Models_List_ShouldPrintJson_WhenJsonModeEnabled()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models", "list", "--json" }));
            if (output.Contains('['))
            {
                var json = output.Substring(output.IndexOf('[')).Trim();
                JsonDocument.Parse(json);
            }
            else
            {
                Assert.Contains("models", output, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void Analyze_ShouldHandleMissingInput()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "analyze" }));
            Assert.Contains("No text", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Validate_ShouldHandleMissingInput()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "validate" }));
            Assert.Contains("No text", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ScrapeModels_ShouldHandleJsonMode()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "scrape-models", "--json" }));
            Assert.True(
                output.Contains("{") ||
                output.Contains("[") ||
                output.Contains("Retrieved", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void UpdateModels_ShouldHandleOpenAIKeyFlag()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "update-models", "--openai-key", "sk-test" }));
            Assert.Contains("Model data", output, StringComparison.OrdinalIgnoreCase);
        }
    }
}
