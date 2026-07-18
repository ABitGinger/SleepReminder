using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using SleepReminder.Services;

namespace SleepReminder.Forms;

public class FullscreenForm : Form
{
    private readonly LogManager _log;
    private Color _textColor = Color.White;
    private Color _bgColor = Color.Black;
    private string _message = "";
    private Rectangle _targetBounds;
    private Rectangle _startBounds;
    private DateTime _enlargeStartTime;
    private int _enlargeDurationMs;
    private bool _isEnlarging;
    private bool _isFullscreen;
    private bool _clickThisFrame;
    private System.Windows.Forms.Timer? _animationTimer;
    private readonly object _lock = new();

    public event Action? OnClicked;

    public FullscreenForm(LogManager log)
    {
        _log = log;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    public void StartEnlarge(Rectangle startBounds, Models.AppSettings settings)
    {
        _startBounds = startBounds;
        _targetBounds = Screen.PrimaryScreen.Bounds;
        _enlargeDurationMs = settings.EnlargeDurationSeconds * 1000;
        _enlargeStartTime = DateTime.UtcNow;
        _isEnlarging = true;
        _isFullscreen = false;
        _message = settings.FullscreenText;
        _textColor = ColorTranslator.FromHtml(settings.FullscreenTextColor);
        _bgColor = ColorTranslator.FromHtml(settings.BackgroundColor);
        Opacity = settings.OpacityPercent / 100.0;

        Bounds = startBounds;
        NativeMethods.ShowWindow(Handle, (int)NativeMethods.SW_SHOWNOACTIVATE);
        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, Left, Top, Width, Height, NativeMethods.SWP_NOACTIVATE);

        EnsureAnimationTimer();
        _animationTimer!.Start();

        _log.Info($"开始放大: {startBounds} -> {_targetBounds}, 时长 {settings.EnlargeDurationSeconds}秒");
    }

    private void EnsureAnimationTimer()
    {
        if (_animationTimer == null)
        {
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animationTimer.Tick += OnAnimationTick;
        }
        else
        {
            _animationTimer.Stop();
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        try
        {
            if (!_isEnlarging) return;

            var elapsed = (DateTime.UtcNow - _enlargeStartTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / _enlargeDurationMs, 1.0);
            var eased = progress;

            var x = (int)(_startBounds.X + (_targetBounds.X - _startBounds.X) * eased);
            var y = (int)(_startBounds.Y + (_targetBounds.Y - _startBounds.Y) * eased);
            var w = (int)(_startBounds.Width + (_targetBounds.Width - _startBounds.Width) * eased);
            var h = (int)(_startBounds.Height + (_targetBounds.Height - _startBounds.Height) * eased);

            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, x, y, w, h, NativeMethods.SWP_NOACTIVATE);

            if (progress >= 1.0)
            {
                _isEnlarging = false;
                _isFullscreen = true;
                _animationTimer?.Stop();
                _log.Info("放大完成，全屏模式");
            }

            Invalidate();
        }
        catch (Exception ex)
        {
            _log.Error($"动画帧异常: {ex.Message}");
        }
    }

    public void ResetToSize(Rectangle startBounds)
    {
        lock (_lock)
        {
            _startBounds = startBounds;
            _targetBounds = Screen.PrimaryScreen.Bounds;
            _enlargeStartTime = DateTime.UtcNow;
            _isEnlarging = true;
            _isFullscreen = false;

            Bounds = startBounds;
            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, Left, Top, Width, Height, NativeMethods.SWP_NOACTIVATE);
            Invalidate();

            EnsureAnimationTimer();
            _animationTimer!.Start();

            _log.Info("重置点击，重新开始放大");
        }
    }

    public void HideWindow()
    {
        if (_animationTimer != null)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer.Dispose();
            _animationTimer = null;
        }
        _isEnlarging = false;
        _isFullscreen = false;
        Hide();
    }

    public bool IsFullscreenMode => _isFullscreen;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(_bgColor);
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using var brush = new SolidBrush(_textColor);
        float fontSize = _isFullscreen ? 36 : Math.Max(12, Height / 4);
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(_message, font, brush, ClientRectangle, sf);

        if (_isFullscreen)
        {
            using var hintFont = new Font("Segoe UI", 14);
            using var hintBrush = new SolidBrush(Color.FromArgb(128, _textColor));
            var hintRect = new Rectangle(0, ClientRectangle.Height - 80, ClientRectangle.Width, 40);
            e.Graphics.DrawString("点击任意位置恢复大小", hintFont, hintBrush, hintRect, sf);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_isFullscreen || _isEnlarging)
        {
            _log.Info("全屏模式点击");
            lock (_lock)
            {
                if (_clickThisFrame) return;
                _clickThisFrame = true;
            }

            try
            {
                var cursorPos = GetCursorPosition();
                Hide();
                Application.DoEvents();
                System.Threading.Thread.Sleep(30);

                if (cursorPos.HasValue)
                {
                    var pt = cursorPos.Value;
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, (uint)pt.X, (uint)pt.Y, 0, UIntPtr.Zero);
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, (uint)pt.X, (uint)pt.Y, 0, UIntPtr.Zero);
                    _log.Debug($"点击穿透转发: ({pt.X}, {pt.Y})");
                }

                OnClicked?.Invoke();

                NativeMethods.ShowWindow(Handle, (int)NativeMethods.SW_SHOWNOACTIVATE);
                NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, Left, Top, Width, Height, NativeMethods.SWP_NOACTIVATE);
            }
            catch (Exception ex)
            {
                _log.Error($"点击穿透转发失败: {ex.Message}");
            }
            finally
            {
                _clickThisFrame = false;
            }
        }
    }

    private System.Drawing.Point? GetCursorPosition()
    {
        if (NativeMethods.GetCursorPos(out var pt))
            return new System.Drawing.Point(pt.X, pt.Y);
        return null;
    }

    protected override bool ShowWithoutActivation => true;

    protected override void WndProc(ref Message m)
    {
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
        {
            m.Result = IntPtr.Zero;
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}