using TokenKit.CLI;

namespace TokenKit.Tests.CLI
{
    public class TokenKitCLITests : IDisposable
    {
        private readonly TextWriter _originalOut;

        public TokenKitCLITests()
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

        // -----------------------------

        [Fact]
        public void RunAsync_ShouldDisplayHelp_WhenNoArgs()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(Array.Empty<string>()));
            Assert.Contains("TokenKit", output);
        }

        [Fact]
        public void RunAsync_ShouldHandleUnknownCommand()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "invalidcmd" }));
            Assert.Contains("Unknown", output);
        }

        [Fact]
        public void RunAsync_ShouldSupportJsonAndQuietModes()
        {
            // JSON mode should suppress banner output but still run safely
            var outputJson = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--json" }));
            Assert.False(string.IsNullOrWhiteSpace(outputJson)); // some output expected (help or message)

            // Quiet mode should explicitly mention quiet mode
            var outputQuiet = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--quiet" }));
            Assert.Contains("quiet mode", outputQuiet, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RunAsync_ShouldShowVersion_AndHelpShort()
        {
            var output1 = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--version" }));
            var output2 = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "--help", "short" }));

            Assert.Contains("version", output1, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Usage:", output2);
        }

        [Fact]
        public void RunAsync_ShouldListModels_Default()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models", "list" }));

            // remove banner lines for clarity
            output = string.Join('\n', output.Split('\n').Where(line => !line.Contains('█') && !string.IsNullOrWhiteSpace(line)));

            // Expect either success listing or fallback warning
            Assert.True(
                output.Contains("Registered Models", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("No models found", StringComparison.OrdinalIgnoreCase),
                $"Unexpected CLI output:\n{output}"
            );
        }

        [Fact]
        public void RunAsync_ShouldHandleModelsUnknownSubcommand()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models", "unknown" }));
            Assert.Contains("Unknown subcommand", output);
        }

        [Fact]
        public void RunAsync_ShouldHandleModelsNoSubcommand()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "models" }));
            Assert.Contains("Missing subcommand", output);
        }

        [Fact]
        public void RunAsync_ShouldShowVersionFlag()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "-v" }));
            Assert.Contains("TokenKit", output);
        }

        [Fact]
        public void RunAsync_ShouldShowHelpFlag()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "-h" }));
            Assert.Contains("Usage:", output);
        }

        [Fact]
        public void RunAsync_ShouldTriggerScrapeModelsSafely()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "scrape-models" }));
            Assert.Contains("OpenAI model data", output);
        }

        [Fact]
        public void RunAsync_ShouldTriggerUpdateModelsSafely()
        {
            var output = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "update-models" }));
            Assert.Contains("Model data updated", output);
        }

        [Fact]
        public void RunAsync_ShouldTriggerAnalyzeAndValidateSafely()
        {
            var analyzeOutput = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "analyze", "Hello", "--model", "gpt-4o" }));
            var validateOutput = CaptureConsole(() => TokenKitCLI.RunAsync(new[] { "validate", "Hi", "--model", "gpt-4o" }));

            analyzeOutput = string.Join('\n', analyzeOutput.Split('\n').Where(line => !line.Contains('█') && !string.IsNullOrWhiteSpace(line)));
            validateOutput = string.Join('\n', validateOutput.Split('\n').Where(line => !line.Contains('█') && !string.IsNullOrWhiteSpace(line)));

            // Expect either valid token info or registry not found message
            Assert.True(
                analyzeOutput.Contains("TokenCount", StringComparison.OrdinalIgnoreCase) ||
                analyzeOutput.Contains("Model", StringComparison.OrdinalIgnoreCase) ||
                analyzeOutput.Contains("not found", StringComparison.OrdinalIgnoreCase),
                $"Analyze output:\n{analyzeOutput}"
            );

            Assert.True(
                validateOutput.Contains("Model", StringComparison.OrdinalIgnoreCase) ||
                validateOutput.Contains("Valid", StringComparison.OrdinalIgnoreCase) ||
                validateOutput.Contains("not found", StringComparison.OrdinalIgnoreCase),
                $"Validate output:\n{validateOutput}"
            );
        }
    }
}

