using System.Text.Json;
using TokenKit.CLI;

namespace TokenKit.Tests.CLI
{
    public class ConsoleStylerTests
    {
        private readonly TextWriter _originalOut;

        public ConsoleStylerTests()
        {
            _originalOut = Console.Out;
        }

        private static string CaptureConsoleOutput(Action action)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            action();
            Console.Out.Flush();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            return sw.ToString();
        }

        [Fact]
        public void WriteSuccess_ShouldWriteGreenMessage()
        {
            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteSuccess("Success message"));

            Assert.Contains("✅", output);
            Assert.Contains("Success message", output);
        }

        [Fact]
        public void WriteWarning_ShouldWriteYellowMessage()
        {
            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteWarning("Be careful!"));

            Assert.Contains("⚠️", output);
            Assert.Contains("Be careful!", output);
        }

        [Fact]
        public void WriteError_ShouldWriteRedMessage()
        {
            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteError("Something went wrong"));

            Assert.Contains("❌", output);
            Assert.Contains("Something went wrong", output);
        }

        [Fact]
        public void WriteInfo_ShouldWritePlainCyanMessage()
        {
            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteInfo("Info line"));

            Assert.Contains("Info line", output);
        }

        [Fact]
        public void WriteJson_ShouldSerializeObject()
        {
            var obj = new { Message = "Hello", Count = 3 };

            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteJson(obj));

            // Trim and find JSON start (skip emojis or color codes)
            var jsonStart = output.IndexOf('{');
            output = jsonStart >= 0 ? output.Substring(jsonStart) : output.Trim();

            using var doc = JsonDocument.Parse(output);
            var root = doc.RootElement;
            Assert.Equal("Hello", root.GetProperty("Message").GetString());
            Assert.Equal(3, root.GetProperty("Count").GetInt32());
        }

        [Fact]
        public void WriteJson_ShouldSupportWarningColorFlag()
        {
            var obj = new { Status = "Partial" };

            var output = CaptureConsoleOutput(() =>
                ConsoleStyler.WriteJson(obj, success: false));

            Assert.Contains("Partial", output);
        }
    }
}
