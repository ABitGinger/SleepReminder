using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SleepReminder.Forms;
using SleepReminder.Models;
using SleepReminder.Services;

namespace SleepReminder;

public class MainApplication : ApplicationContext
{
    private readonly LogManager _log = new();
    private readonly SettingsManager _settingsManager;
    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.Timer _mainTimer;
    private CountdownForm? _countdownForm;
    private FullscreenForm? _fullscreenForm;
    private LogForm? _logForm;
    private AppState _currentState = AppState.Idle;

    private enum AppState
    {
        Idle,
        Countdown,
        Enlarging,
        Fullscreen
    }

    public MainApplication(bool firstRun = false)
    {
        _log.Info("应用程序启动");
        _settingsManager = new SettingsManager(_log);

        _mainTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _mainTimer.Tick += MainTimer_Tick;
        _mainTimer.Start();
        _log.Info("定时器已启动");

        _trayIcon = new NotifyIcon
        {
            Icon = IconHelper.CreateSleepIcon(),
            Text = "SleepReminder\n睡眠提醒",
            Visible = true
        };
        BuildTrayMenu();
        _log.Info("系统托盘图标已初始化");

        CheckSchedule();

        if (firstRun)
        {
            _log.Info("首次运行，自动弹出设置窗口");
            System.Windows.Forms.Timer? delayTimer = null;
            delayTimer = new System.Windows.Forms.Timer { Interval = 500 };
            delayTimer.Tick += (_, _) =>
            {
                delayTimer?.Stop();
                delayTimer?.Dispose();
                ShowSettings();
            };
            delayTimer.Start();
        }
    }

    private void BuildTrayMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Font = new Font("Segoe UI", 10);

        menu.Items.Add("打开设置", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());

        var debugGroup = new ToolStripMenuItem("调试");
        debugGroup.DropDownItems.Add("查看运行日志", null, (_, _) => ShowLogForm());
        if (_settingsManager.Current.DebugMode)
        {
            debugGroup.DropDownItems.Add(new ToolStripSeparator());
            debugGroup.DropDownItems.Add("显示倒计时", null, (_, _) => DebugShowCountdown());
            debugGroup.DropDownItems.Add("显示全屏", null, (_, _) => DebugShowFullscreen());
            debugGroup.DropDownItems.Add("重置全屏", null, (_, _) => DebugResetFullscreen());
            debugGroup.DropDownItems.Add("重新检查", null, (_, _) => DebugRecheck());
            debugGroup.DropDownItems.Add("隐藏全部", null, (_, _) => DebugHideAll());
        }
        menu.Items.Add(debugGroup);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => OnExit());

        _trayIcon.ContextMenuStrip = menu;
    }

    private void ShowSettings()
    {
        var form = new SettingsForm(_settingsManager.Current, _log, savedSettings =>
        {
            bool debugChanged = savedSettings.DebugMode != _settingsManager.Current.DebugMode;
            SystemHelper.SetAutoStart(savedSettings.AutoStart);
            _settingsManager.Current = savedSettings;
            _settingsManager.SaveSettings();
            if (!savedSettings.DebugMode && debugChanged)
            {
                HideAllForms();
                _currentState = AppState.Idle;
            }
            CheckSchedule();
            BuildTrayMenu();
            _log.Info($"设置更新完成: {savedSettings.ShowHour:D2}:{savedSettings.ShowMinute:D2} - "
                + $"{savedSettings.TargetHour:D2}:{savedSettings.TargetMinute:D2} (目标) - "
                + $"{savedSettings.HideHour:D2}:{savedSettings.HideMinute:D2} (隐藏)");
        });
        form.ShowDialog();
    }

    private void ShowLogForm()
    {
        if (_logForm == null || _logForm.IsDisposed)
        {
            _logForm = new LogForm(_log);
            _logForm.FormClosed += (_, _) => _logForm = null;
            _logForm.Show();
            _log.Info("日志窗口已打开");
        }
        else
        {
            _logForm.Activate();
        }
    }

    private void MainTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            CheckSchedule();
        }
        catch (Exception ex)
        {
            _log.Error($"定时器触发异常: {ex.Message}");
        }
    }

    private void CheckSchedule()
    {
        if (_settingsManager.Current.DebugMode)
            return;

        var now = DateTime.Now.TimeOfDay;
        var settings = _settingsManager.Current;

        if (settings.IsInCountdownPeriod(now))
        {
            if (_currentState != AppState.Countdown)
            {
                _log.Info($"状态切换: {_currentState} -> Countdown (倒计时时段)");
                HideAllForms();
                ShowCountdownForm();
                _currentState = AppState.Countdown;
            }
            UpdateCountdownDisplay(now, settings);
        }
        else if (settings.IsInFullscreenPeriod(now))
        {
            if (_currentState != AppState.Fullscreen && _currentState != AppState.Enlarging)
            {
                _log.Info($"状态切换: {_currentState} -> Fullscreen (目标时间到)");
                HideAllForms();
                ShowFullscreenForm(settings);
                _currentState = AppState.Enlarging;
            }
        }
        else
        {
            if (_currentState != AppState.Idle)
            {
                _log.Info($"状态切换: {_currentState} -> Idle (隐藏时间到)");
                HideAllForms();
                _currentState = AppState.Idle;
            }
        }
    }

    private void ShowCountdownForm()
    {
        if (_countdownForm == null || _countdownForm.IsDisposed)
        {
            _countdownForm = new CountdownForm(_log);
            _countdownForm.FormClosed += (_, _) => _countdownForm = null;
        }
        _countdownForm.ApplySettings(_settingsManager.Current);
        _countdownForm.ShowWithoutActivate();
        _log.Debug($"倒计时窗口显示: 位置=({_countdownForm.Left},{_countdownForm.Top}) "
            + $"大小=({_countdownForm.Width}x{_countdownForm.Height})");
    }

    private void UpdateCountdownDisplay(TimeSpan now, Models.AppSettings settings)
    {
        if (_countdownForm == null || _countdownForm.IsDisposed) return;
        var remaining = settings.GetCountdownRemaining(now);
        _countdownForm.UpdateCountdown(remaining);
    }

    private void ShowFullscreenForm(Models.AppSettings settings)
    {
        if (_fullscreenForm == null || _fullscreenForm.IsDisposed)
        {
            _fullscreenForm = new FullscreenForm(_log);
            _fullscreenForm.FormClosed += (_, _) => _fullscreenForm = null;
            _fullscreenForm.OnClicked += OnFullscreenClicked;
        }

        var startBounds = GetCountdownBounds(settings);
        _fullscreenForm.StartEnlarge(startBounds, settings);
        _log.Info($"全屏窗口显示，起始位置: {startBounds}");
    }

    private Rectangle GetCountdownBounds(Models.AppSettings settings)
    {
        if (settings.WindowX >= 0 && settings.WindowY >= 0)
            return new Rectangle(settings.WindowX, settings.WindowY, settings.WindowWidth, settings.WindowHeight);

        var screen = Screen.PrimaryScreen.WorkingArea;
        return new Rectangle(
            screen.Right - settings.WindowWidth - 10,
            screen.Bottom - settings.WindowHeight - 10,
            settings.WindowWidth,
            settings.WindowHeight);
    }

    private void OnFullscreenClicked()
    {
        _log.Info("全屏模式点击，重置大小");
        var bounds = GetCountdownBounds(_settingsManager.Current);
        _fullscreenForm?.ResetToSize(bounds);
        _currentState = AppState.Enlarging;
    }

    private void HideAllForms()
    {
        if (_countdownForm != null && !_countdownForm.IsDisposed)
        {
            _countdownForm.Hide();
            _log.Debug("倒计时窗口已隐藏");
        }
        if (_fullscreenForm != null && !_fullscreenForm.IsDisposed)
        {
            _fullscreenForm.HideWindow();
            _log.Debug("全屏窗口已隐藏");
        }
    }

    private void OnExit()
    {
        _log.Info("用户发起退出程序");
        using (var reasonForm = new ExitReasonForm())
        {
            reasonForm.ShowDialog();
            if (!reasonForm.Submitted) return;

            var record = new ExitRecord { Reason = reasonForm.Reason, ExitTime = DateTime.Now };
            _settingsManager.SaveExitRecord(record);
            _log.Info($"退出理由已提交: {reasonForm.Reason}");
        }

        using (var historyForm = new ExitHistoryForm(_settingsManager.LoadExitRecords()))
        {
            historyForm.ShowDialog();
        }

        _log.Info("应用程序退出");
        CleanupResources();
        Application.Exit();
    }

    private void CleanupResources()
    {
        _mainTimer.Stop();
        _mainTimer.Dispose();

        if (_countdownForm != null && !_countdownForm.IsDisposed)
            _countdownForm.Dispose();
        if (_fullscreenForm != null && !_fullscreenForm.IsDisposed)
            _fullscreenForm.Dispose();
        if (_logForm != null && !_logForm.IsDisposed)
            _logForm.Dispose();

        _trayIcon.Visible = false;
        _trayIcon.Dispose();
    }

    // Debug methods
    private void DebugShowCountdown()
    {
        _log.Info("[调试] 强制显示倒计时");
        HideAllForms();
        ShowCountdownForm();
        _currentState = AppState.Countdown;
    }

    private void DebugShowFullscreen()
    {
        _log.Info("[调试] 强制显示全屏");
        HideAllForms();
        ShowFullscreenForm(_settingsManager.Current);
        _currentState = AppState.Enlarging;
    }

    private void DebugResetFullscreen()
    {
        _log.Info("[调试] 强制重置全屏");
        OnFullscreenClicked();
    }

    private void DebugRecheck()
    {
        _log.Info("[调试] 强制定时检查");
        CheckSchedule();
    }

    private void DebugHideAll()
    {
        _log.Info("[调试] 隐藏全部窗口");
        HideAllForms();
        _currentState = AppState.Idle;
    }
}