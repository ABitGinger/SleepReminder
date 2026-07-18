using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SleepReminder.Services;

public static class SystemHelper
{
    private const string AppName = "SleepReminder";
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey, false);
            if (key == null) return false;
            var value = key.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch { return false; }
    }

    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (key == null) return;
            if (enable)
            {
                var exePath = GetExecutablePath();
                if (!string.IsNullOrEmpty(exePath))
                    key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(AppName) != null)
                    key.DeleteValue(AppName);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set auto-start: {ex.Message}");
        }
    }

    public static bool PinToStartMenu()
    {
        try
        {
            var exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath)) return false;

            var startMenuPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs",
                "SleepReminder.lnk");

            if (File.Exists(startMenuPath))
            {
                return true;
            }

            CreateShortcut(exePath, startMenuPath, "SleepReminder - 睡眠提醒");
            return File.Exists(startMenuPath);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsPinnedToStartMenu()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            "Programs",
            "SleepReminder.lnk");
        return File.Exists(path);
    }

    private static string? GetExecutablePath()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path) && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            var exe = path.Substring(0, path.Length - 4) + ".exe";
            if (File.Exists(exe)) return exe;
        }
        return path;
    }

    private static void CreateShortcut(string targetPath, string shortcutPath, string description)
    {
        Type? shellType = null;
        object? shell = null;
        object? shortcut = null;
        try
        {
            shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            shortcut = shellType.InvokeMember("CreateShortcut",
                System.Reflection.BindingFlags.InvokeMethod, null, shell,
                new object[] { shortcutPath });

            if (shortcut != null)
            {
                var t = shortcut.GetType();
                t.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                t.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { description });
                t.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
            }
        }
        finally
        {
            if (shortcut != null) Marshal.ReleaseComObject(shortcut);
            if (shell != null) Marshal.ReleaseComObject(shell);
        }
    }
}
