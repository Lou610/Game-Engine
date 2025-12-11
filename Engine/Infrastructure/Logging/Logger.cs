using System;
using System.IO;
using Engine.Domain.Core;

namespace Engine.Infrastructure.Logging;

/// <summary>
/// Infrastructure service for logging
/// </summary>
public class Logger
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new();

    public Logger(string logFilePath = "engine.log")
    {
        _logFilePath = logFilePath;
    }

    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [{level}] {message}";

        if (exception != null)
        {
            logMessage += $"\n{exception}";
        }

        // Console output
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetConsoleColor(level);
        Console.WriteLine(logMessage);
        Console.ForegroundColor = originalColor;

        // File output
        lock (_lockObject)
        {
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
        }
    }

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);
    public void Critical(string message, Exception? exception = null) => Log(LogLevel.Critical, message, exception);

    private static ConsoleColor GetConsoleColor(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.Gray,
        LogLevel.Debug => ConsoleColor.Cyan,
        LogLevel.Info => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.Magenta,
        _ => ConsoleColor.White
    };
}

