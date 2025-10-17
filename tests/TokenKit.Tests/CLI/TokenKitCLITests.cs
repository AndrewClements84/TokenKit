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
    }
}

