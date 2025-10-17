using System.Text;

namespace TokenKit.CLI;

public static class Logger
{
    private static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "tokenkit.log");
    private static readonly object _lock = new();

    public static bool QuietMode { get; set; }

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);
    public static void Success(string message) => Write("SUCCESS", message);

    private static void Write(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";

        lock (_lock)
        {
            File.AppendAllText(LogFile, line, Encoding.UTF8);

            // truncate if larger than 1 MB
            var fi = new FileInfo(LogFile);
            if (fi.Length > 1_000_000)
            {
                var lines = File.ReadAllLines(LogFile);
                File.WriteAllLines(LogFile, lines.Skip(lines.Length / 2));
            }
        }

        if (!QuietMode)
        {
            switch (level)
            {
                case "ERROR": ConsoleStyler.WriteError(message); break;
                case "WARN": ConsoleStyler.WriteWarning(message); break;
                case "SUCCESS": ConsoleStyler.WriteSuccess(message); break;
                default: ConsoleStyler.WriteInfo(message); break;
            }
        }
    }
}

