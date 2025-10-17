using TokenKit.CLI;

namespace TokenKit.Tests.CLI
{
    public class LoggerTests : IDisposable
    {
        private readonly string _logFile;
        private readonly TextWriter _originalOut;

        public LoggerTests()
        {
            _originalOut = Console.Out;
            _logFile = Path.Combine(AppContext.BaseDirectory, "tokenkit.log");
            if (File.Exists(_logFile))
                File.Delete(_logFile);
            Logger.QuietMode = false;
        }

        public void Dispose()
        {
            // restore console and quiet mode after each test
            Console.SetOut(_originalOut);
            Logger.QuietMode = false;
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
        public void Info_Warn_Error_Success_ShouldWriteToFile_AndConsole()
        {
            // Act
            var output = CaptureConsoleOutput(() =>
            {
                Logger.Info("info test");
                Logger.Warn("warn test");
                Logger.Error("error test");
                Logger.Success("success test");
            });

            // Assert console output
            Assert.Contains("info test", output);
            Assert.Contains("warn test", output);
            Assert.Contains("error test", output);
            Assert.Contains("success test", output);

            // Assert file contents
            Assert.True(File.Exists(_logFile));
            var log = File.ReadAllText(_logFile);
            Assert.Contains("INFO", log);
            Assert.Contains("WARN", log);
            Assert.Contains("ERROR", log);
            Assert.Contains("SUCCESS", log);
        }

        [Fact]
        public void ShouldRespectQuietMode_AndStillWriteFile()
        {
            Logger.QuietMode = true;

            var output = CaptureConsoleOutput(() =>
            {
                Logger.Info("quiet test");
            });

            Assert.True(string.IsNullOrWhiteSpace(output)); // no console output
            Assert.True(File.Exists(_logFile));
            Assert.Contains("quiet test", File.ReadAllText(_logFile));
        }

        [Fact]
        public void ShouldTruncateLog_WhenExceedsOneMB()
        {
            // Arrange — create a large dummy log file
            var largeLines = string.Join(Environment.NewLine, Enumerable.Repeat("X", 150000));
            File.WriteAllText(_logFile, largeLines);

            // Act
            Logger.Info("trigger truncate");

            // Assert file shrinks roughly in half
            var newSize = new FileInfo(_logFile).Length;
            Assert.True(newSize < 1_000_000);
            Assert.Contains("trigger truncate", File.ReadAllText(_logFile));
        }
    }
}
