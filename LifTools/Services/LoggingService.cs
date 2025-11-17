using System;
using System.IO;
using System.Threading;

namespace LifTools.Services;

public class LoggingService
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public LoggingService()
    {
        // Store logs in a log subdirectory next to the executable
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logFolder = Path.Combine(appDirectory, "log");
        
        // Ensure the log directory exists
        Directory.CreateDirectory(logFolder);
        
        // Create log file with date in name
        var logFileName = $"lifTools_{DateTime.Now:yyyy-MM-dd}.log";
        _logFilePath = Path.Combine(logFolder, logFileName);
    }

    public void LogError(string message, Exception? exception = null)
    {
        Log("ERROR", message, exception);
    }

    public void LogWarning(string message, Exception? exception = null)
    {
        Log("WARNING", message, exception);
    }

    public void LogInfo(string message)
    {
        Log("INFO", message, null);
    }

    private void Log(string level, string message, Exception? exception)
    {
        try
        {
            lock (_lockObject)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}";
                
                if (exception != null)
                {
                    logEntry += $"\n  Exception: {exception.GetType().Name}: {exception.Message}";
                    if (exception.StackTrace != null)
                    {
                        logEntry += $"\n  StackTrace: {exception.StackTrace}";
                    }
                    if (exception.InnerException != null)
                    {
                        logEntry += $"\n  InnerException: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
                    }
                }
                
                logEntry += Environment.NewLine;
                
                File.AppendAllText(_logFilePath, logEntry);
            }
        }
        catch
        {
            // Silently fail if we can't write to the log file
            // We don't want logging failures to crash the application
        }
    }
}

