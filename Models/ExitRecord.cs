using System;
using System.Text.Json.Serialization;

namespace SleepReminder.Models;

public class ExitRecord
{
    public string Reason { get; set; } = string.Empty;
    public DateTime ExitTime { get; set; } = DateTime.Now;

    public override string ToString()
        => $"[{ExitTime:yyyy-MM-dd HH:mm:ss}] {Reason}";
}