using System;
using System.Text;
using System.Windows;

namespace Rewriter.Logging
{
    public static class Logger
    {
        public static event Action<string> CurrentLogChanged;

        public static string CurrentLog { get; private set; }

        public static void Debug(string message) => Log(FormatLogMessage(nameof(Debug), message));

        public static void Info(string message) => Log(FormatLogMessage(nameof(Info), message));

        public static void Warn(string message) => Log(FormatLogMessage(nameof(Warn), message));

        public static void Error(string message) => Log(FormatLogMessage(nameof(Error), message));

        public static void Warn(Exception exception, string message = null) => Log(FormatLogMessage(nameof(Warn), FormatException(exception, message)));

        public static void Error(Exception exception, string message = null) => Log(FormatLogMessage(nameof(Error), FormatException(exception, message)));

        private static void Log(string message)
        {
            if (CurrentLog != null)
            {
                message = Environment.NewLine + message;
            }
            
            CurrentLog += message;
            CurrentLogChanged?.Invoke(message);
        }

        private static string FormatException(Exception exception, string message) => $"{message ?? exception.GetType().Name}: {exception}";

        private static string FormatLogMessage(string level, string message) => $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}";
    }
}