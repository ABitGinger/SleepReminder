using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SleepReminder.Models;

namespace SleepReminder.Services;

public class SettingsManager
{
    private readonly string _settingsPath;
    private readonly string _exitHistoryPath;
    private readonly LogManager _log;

    public AppSettings Current { get; set; }

    public SettingsManager(LogManager log)
    {
        _log = log;
        var appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SleepReminder");
        Directory.CreateDirectory(appDir);
        _settingsPath = Path.Combine(appDir, "settings.json");
        _exitHistoryPath = Path.Combine(appDir, "exit_history.json");
        Current = LoadSettings();
    }

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null && settings.Validate(out _))
                {
                    _log.Info($"设置加载成功: show={settings.ShowHour:D2}:{settings.ShowMinute:D2}, "
                        + $"target={settings.TargetHour:D2}:{settings.TargetMinute:D2}, "
                        + $"hide={settings.HideHour:D2}:{settings.HideMinute:D2}");
                    return settings;
                }
                _log.Warn("设置加载失败，使用默认设置");
            }
            else
            {
                _log.Info("未找到设置文件，使用默认设置");
            }
        }
        catch (Exception ex)
        {
            _log.Error($"读取设置文件失败: {ex.Message}");
        }
        return new AppSettings();
    }

    public void SaveSettings()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Current, options);
            File.WriteAllText(_settingsPath, json);
            _log.Info("设置保存成功");
        }
        catch (Exception ex)
        {
            _log.Error($"设置保存失败: {ex.Message}");
        }
    }

    public void SaveExitRecord(ExitRecord record)
    {
        try
        {
            var records = new List<ExitRecord>();
            if (File.Exists(_exitHistoryPath))
            {
                var json = File.ReadAllText(_exitHistoryPath);
                var existing = JsonSerializer.Deserialize<List<ExitRecord>>(json);
                if (existing != null) records = existing;
            }
            records.Add(record);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_exitHistoryPath, JsonSerializer.Serialize(records, options));
            _log.Info($"退出记录已保存: {record.Reason}");
        }
        catch (Exception ex)
        {
            _log.Error($"保存退出记录失败: {ex.Message}");
        }
    }

    public List<ExitRecord> LoadExitRecords()
    {
        try
        {
            if (File.Exists(_exitHistoryPath))
            {
                var json = File.ReadAllText(_exitHistoryPath);
                var records = JsonSerializer.Deserialize<List<ExitRecord>>(json);
                if (records != null) return records;
            }
        }
        catch (Exception ex)
        {
            _log.Error($"读取退出记录失败: {ex.Message}");
        }
        return new List<ExitRecord>();
    }
}