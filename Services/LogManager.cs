using System;
using System.Collections.Generic;
using System.Text;

namespace SleepReminder.Services;

public class LogManager
{
    private readonly List<string> _logs = new();
    private readonly object _lock = new();
    private const int MaxLogs = 10000;

    public event Action<string>? OnLogAdded;

    public void Info(string message) => AddEntry("INFO", message);
    public void Warn(string message) => AddEntry("WARN", message);
    public void Error(string message) => AddEntry("ERROR", message);
    public void Debug(string message) => AddEntry("DEBUG", message);

    private void AddEntry(string level, string message)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        lock (_lock)
        {
            _logs.Add(entry);
            if (_logs.Count > MaxLogs)
                _logs.RemoveRange(0, _logs.Count - MaxLogs);
        }
        OnLogAdded?.Invoke(entry);
    }

    public string GetAllLogs()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            foreach (var log in _logs)
                sb.AppendLine(log);
            return sb.ToString();
        }
    }

    public string GetRecentLogs(int count = 200)
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            int start = Math.Max(0, _logs.Count - count);
            for (int i = start; i < _logs.Count; i++)
                sb.AppendLine(_logs[i]);
            return sb.ToString();
        }
    }

    public IReadOnlyList<string> GetLogList()
    {
        lock (_lock)
        {
            return _logs.ToArray();
        }
    }
}