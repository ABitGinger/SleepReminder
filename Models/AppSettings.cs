using System;
using System.Text.Json.Serialization;

namespace SleepReminder.Models;

public class AppSettings
{
    public int ShowHour { get; set; } = 22;
    public int ShowMinute { get; set; } = 0;
    public int TargetHour { get; set; } = 23;
    public int TargetMinute { get; set; } = 0;
    public int HideHour { get; set; } = 5;
    public int HideMinute { get; set; } = 0;
    public int WindowWidth { get; set; } = 200;
    public int WindowHeight { get; set; } = 60;
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;
    public int OpacityPercent { get; set; } = 50;
    public int EnlargeDurationSeconds { get; set; } = 60;
    public bool DebugMode { get; set; } = false;
    public string FontName { get; set; } = "Segoe UI";
    public float FontSize { get; set; } = 24f;
    public string CountdownTextColor { get; set; } = "#FFFFFF";
    public string FullscreenText { get; set; } = "该睡觉了，点击任意位置恢复";
    public string FullscreenTextColor { get; set; } = "#FFFFFF";
    public string BackgroundColor { get; set; } = "#000000";
    public bool AutoStart { get; set; } = false;

    public bool Validate(out string error)
    {
        error = string.Empty;

        if (ShowHour < 0 || ShowHour > 23 || ShowMinute < 0 || ShowMinute > 59)
        { error = "显示时间不合法"; return false; }
        if (TargetHour < 0 || TargetHour > 23 || TargetMinute < 0 || TargetMinute > 59)
        { error = "目标时间不合法"; return false; }
        if (HideHour < 0 || HideHour > 23 || HideMinute < 0 || HideMinute > 59)
        { error = "隐藏时间不合法"; return false; }
        if (WindowWidth < 50 || WindowWidth > 2000)
        { error = "窗口宽度应在50-2000之间"; return false; }
        if (WindowHeight < 20 || WindowHeight > 2000)
        { error = "窗口高度应在20-2000之间"; return false; }
        if (OpacityPercent < 5 || OpacityPercent > 100)
        { error = "透明度应在5-100之间"; return false; }
        if (EnlargeDurationSeconds < 10 || EnlargeDurationSeconds > 600)
        { error = "放大时长应在10-600秒之间"; return false; }
        if (FontSize < 8 || FontSize > 200)
        { error = "字号应在8-200之间"; return false; }

        var show = new TimeSpan(ShowHour, ShowMinute, 0);
        var target = new TimeSpan(TargetHour, TargetMinute, 0);
        var hide = new TimeSpan(HideHour, HideMinute, 0);

        if (show >= target)
        { error = "显示时间必须在目标时间之前"; return false; }
        if (hide == show)
        { error = "隐藏时间不能与显示时间相同"; return false; }
        if (hide == target)
        { error = "隐藏时间不能与目标时间相同"; return false; }

        return true;
    }

    public TimeSpan GetShowTime() => new(ShowHour, ShowMinute, 0);
    public TimeSpan GetTargetTime() => new(TargetHour, TargetMinute, 0);
    public TimeSpan GetHideTime() => new(HideHour, HideMinute, 0);

    public bool IsInActivePeriod(TimeSpan now)
    {
        var show = GetShowTime();
        var hide = GetHideTime();
        if (show < hide)
            return now >= show && now < hide;
        else
            return now >= show || now < hide;
    }

    public bool IsInCountdownPeriod(TimeSpan now)
    {
        var show = GetShowTime();
        var target = GetTargetTime();
        if (show < target)
            return now >= show && now < target;
        else
            return now >= show || now < target;
    }

    public bool IsInFullscreenPeriod(TimeSpan now)
    {
        var target = GetTargetTime();
        var hide = GetHideTime();
        if (target < hide)
            return now >= target && now < hide;
        else
            return now >= target || now < hide;
    }

    public TimeSpan GetCountdownRemaining(TimeSpan now)
    {
        var target = GetTargetTime();
        if (now <= target)
            return target - now;
        return target.Add(new TimeSpan(24, 0, 0)) - now;
    }

    public AppSettings Clone()
    {
        return new AppSettings
        {
            ShowHour = ShowHour,
            ShowMinute = ShowMinute,
            TargetHour = TargetHour,
            TargetMinute = TargetMinute,
            HideHour = HideHour,
            HideMinute = HideMinute,
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            WindowX = WindowX,
            WindowY = WindowY,
            OpacityPercent = OpacityPercent,
            EnlargeDurationSeconds = EnlargeDurationSeconds,
            DebugMode = DebugMode,
            FontName = FontName,
            FontSize = FontSize,
            CountdownTextColor = CountdownTextColor,
            FullscreenText = FullscreenText,
            FullscreenTextColor = FullscreenTextColor,
            BackgroundColor = BackgroundColor,
            AutoStart = AutoStart
        };
    }
}