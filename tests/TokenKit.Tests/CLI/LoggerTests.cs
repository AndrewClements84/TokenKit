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
            // Keep console quiet so file I/O is deterministic
            Logger.QuietMode = true;

            // Build a log file just over 1 MB with MANY numbered lines
            // so File.ReadAllLines(...) returns a large array we can verify.
            var payload = new string('X', 80);
            using (var sw = new StreamWriter(_logFile, false))
            {
                int i = 0;
                while (new FileInfo(_logFile).Length <= 1_050_000) // a bit above 1MB to guarantee the branch
                {
                    sw.WriteLine($"LINE {i:D6} {payload}");
                    i++;
                    if (i % 1000 == 0) sw.Flush(); // help file size update during loop
                }
            }

            var beforeLines = File.ReadAllLines(_logFile).Length;
            Assert.True(beforeLines > 0, "Precondition: file should contain lines");

            // Act: write one more entry -> triggers truncation branch
            Logger.Info("trigger truncate");

            // Assert: line count should be roughly half + the appended line
            var afterLines = File.ReadAllLines(_logFile);
            Assert.True(afterLines.Length < beforeLines, "Log file should have been truncated");
            Assert.InRange(afterLines.Length, beforeLines / 2, beforeLines / 2 + 100);

            // Earliest lines should be gone; appended line should be present
            Assert.DoesNotContain(afterLines, l => l.Contains("LINE 000000"));
            Assert.Contains(afterLines, l => l.Contains("trigger truncate"));

            // Reset quiet mode for other tests
            Logger.QuietMode = false;
        }

    }
}
