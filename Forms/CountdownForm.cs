using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using SleepReminder.Services;

namespace SleepReminder.Forms;

public class CountdownForm : Form
{
    private readonly LogManager _log;
    private string _displayText = "00:00";
    private Color _textColor = Color.White;
    private Color _bgColor = Color.Black;

    public CountdownForm(LogManager log)
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
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    public void ApplySettings(Models.AppSettings settings)
    {
        Size = new Size(settings.WindowWidth, settings.WindowHeight);
        Opacity = settings.OpacityPercent / 100.0;
        _textColor = ColorTranslator.FromHtml(settings.CountdownTextColor);
        _bgColor = ColorTranslator.FromHtml(settings.BackgroundColor);

        if (settings.WindowX >= 0 && settings.WindowY >= 0)
        {
            Location = new Point(settings.WindowX, settings.WindowY);
        }
        else
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(screen.Right - settings.WindowWidth - 10, screen.Bottom - settings.WindowHeight - 10);
        }

        Invalidate();
    }

    public void UpdateCountdown(TimeSpan remaining)
    {
        _displayText = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
        Invalidate();
    }

    public void ShowWithoutActivate()
    {
        NativeMethods.ShowWindow(Handle, (int)NativeMethods.SW_SHOWNOACTIVATE);
        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, Left, Top, Width, Height, NativeMethods.SWP_NOACTIVATE);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(_bgColor);
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using var brush = new SolidBrush(_textColor);
        using var font = new Font("Segoe UI", 20, FontStyle.Bold);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(_displayText, font, brush, ClientRectangle, sf);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        if (m.Msg == WM_NCHITTEST)
        {
            m.Result = new IntPtr(-1);
            return;
        }

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

    protected override bool ShowWithoutActivation => true;
}