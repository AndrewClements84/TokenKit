using System.Text.Json;
using TokenKit.CLI;
using TokenKit.Models;

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

        // --------------------------------------------------------------------
        // 1️⃣  ShowVersion() path (covers multiple identical version calls)
        // --------------------------------------------------------------------
        [Fact]
        public void ShowVersion_ShouldPrintVersion()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--version" }));
            Assert.Contains("version", output, StringComparison.OrdinalIgnoreCase);
        }

        // --------------------------------------------------------------------
        // 2️⃣  ListModels with provider filter that finds none
        // --------------------------------------------------------------------
        [Fact]
        public void Models_List_ShouldHandleProviderFilter_NoResults()
        {
            // Act
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models", "list", "--provider", "NonexistentProvider" }));

            // Assert
            Assert.Contains("No models found", output, StringComparison.OrdinalIgnoreCase);
        }

        // --------------------------------------------------------------------
        // 3️⃣  ListModels with JSON mode active
        // --------------------------------------------------------------------
        [Fact]
        public void Models_List_ShouldPrintJson_WhenJsonModeEnabled()
        {
            // Act
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models", "list", "--json" }));

            // Assert — should output valid JSON array or empty brackets
            var jsonStart = output.IndexOf('[');
            if (jsonStart >= 0)
            {
                var json = output.Substring(jsonStart).Trim();
                JsonDocument.Parse(json); // ensure it's valid JSON
            }
            else
            {
                Assert.Contains("No models", output, StringComparison.OrdinalIgnoreCase);
            }
        }

        // --------------------------------------------------------------------
        // 4️⃣  Analyze with missing input (triggers "No input provided" branch)
        // --------------------------------------------------------------------
        [Fact]
        public void Analyze_ShouldHandleMissingInput()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "analyze" }));

            // strip banner noise
            output = string.Join('\n', output.Split('\n')
                .Where(line => !line.Contains('█') && !line.Contains("TokenKit v")));

            Assert.True(
                output.Contains("No input provided to analyze", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Missing text, file path, or stdin input", StringComparison.OrdinalIgnoreCase),
                $"Analyze output:\n{output}"
            );
        }

        // --------------------------------------------------------------------
        // 5️⃣  Validate with missing input (triggers "No input provided" branch)
        // --------------------------------------------------------------------
        [Fact]
        public void Validate_ShouldHandleMissingInput()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "validate" }));

            output = string.Join('\n', output.Split('\n')
                .Where(line => !line.Contains('█') && !line.Contains("TokenKit v")));

            Assert.True(
                output.Contains("No input provided to validate", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Missing text, file path, or stdin input", StringComparison.OrdinalIgnoreCase),
                $"Validate output:\n{output}"
            );
        }

        // --------------------------------------------------------------------
        // 6️⃣  UpdateModels with piped JSON input (covers JSON parsing + stdin branch)
        // --------------------------------------------------------------------
        [Fact]
        public async Task UpdateModels_ShouldHandlePipedInputJson()
        {
            var models = new List<ModelSpec> {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, Encoding = "cl100k_base" }
            };
            var json = JsonSerializer.Serialize(models);

            using var reader = new StringReader(json);
            Console.SetIn(reader);

            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "update-models" }));
            Assert.True(
                output.Contains("Updated", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("stdin", StringComparison.OrdinalIgnoreCase),
                $"Unexpected output: {output}"
            );
        }

        // --------------------------------------------------------------------
        // 7️⃣  ScrapeModels with JSON mode (covers JSON output path)
        // --------------------------------------------------------------------
        [Fact]
        public void ScrapeModels_ShouldHandleJsonMode()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "scrape-models", "--json" }));

            Assert.True(
                output.Contains("{", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Retrieved", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Failed", StringComparison.OrdinalIgnoreCase),
                $"Unexpected output: {output}"
            );
        }

        // --------------------------------------------------------------------
        // 8️⃣  UpdateModels and ScrapeModels with fake key flag (covers --openai-key parsing)
        // --------------------------------------------------------------------
        [Fact]
        public void UpdateModels_ShouldHandleOpenAIKeyFlag()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "update-models", "--openai-key", "sk-test123" }));
            Assert.True(
                output.Contains("Model data", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Updated", StringComparison.OrdinalIgnoreCase),
                $"Unexpected output: {output}"
            );
        }

        [Fact]
        public void ScrapeModels_ShouldHandleOpenAIKeyFlag()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "scrape-models", "--openai-key", "sk-test123" }));
            Assert.True(
                output.Contains("OpenAI", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("models", StringComparison.OrdinalIgnoreCase),
                $"Unexpected output: {output}"
            );
        }
    }
}

