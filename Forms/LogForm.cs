using System;
using System.Drawing;
using System.Windows.Forms;
using SleepReminder.Services;

namespace SleepReminder.Forms;

public class LogForm : Form
{
    private readonly LogManager _log;
    private readonly TextBox _txtLog;
    private readonly CheckBox _chkAutoScroll;

    public LogForm(LogManager log)
    {
        _log = log;
        Text = "SleepReminder - 运行日志";
        Size = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Consolas", 9);
        MinimizeBox = false;
        ShowInTaskbar = false;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            Padding = new Padding(6)
        };

        var btnCopy = new Button
        {
            Text = "一键复制全部日志",
            Location = new Point(6, 4),
            Size = new Size(150, 28),
            Font = new Font("Segoe UI", 9)
        };
        btnCopy.Click += BtnCopy_Click;
        topPanel.Controls.Add(btnCopy);

        var btnRefresh = new Button
        {
            Text = "刷新",
            Location = new Point(166, 4),
            Size = new Size(80, 28),
            Font = new Font("Segoe UI", 9)
        };
        btnRefresh.Click += (_, _) => RefreshLogs();
        topPanel.Controls.Add(btnRefresh);

        _chkAutoScroll = new CheckBox
        {
            Text = "自动滚动",
            Location = new Point(260, 6),
            Size = new Size(90, 24),
            Checked = true,
            Font = new Font("Segoe UI", 9)
        };
        topPanel.Controls.Add(_chkAutoScroll);

        _txtLog = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = Font,
            BackColor = Color.Black,
            ForeColor = Color.Lime
        };

        Controls.Add(_txtLog);
        Controls.Add(topPanel);

        _log.OnLogAdded += OnNewLog;
        RefreshLogs();
    }

    private void OnNewLog(string entry)
    {
        if (IsDisposed) return;
        try
        {
            BeginInvoke(() =>
            {
                _txtLog.AppendText(entry + Environment.NewLine);
                if (_chkAutoScroll.Checked)
                {
                    _txtLog.SelectionStart = _txtLog.TextLength;
                    _txtLog.ScrollToCaret();
                }
            });
        }
        catch { }
    }

    private void RefreshLogs()
    {
        _txtLog.Clear();
        foreach (var line in _log.GetLogList())
            _txtLog.AppendText(line + Environment.NewLine);
        if (_chkAutoScroll.Checked)
        {
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.ScrollToCaret();
        }
    }

    private void BtnCopy_Click(object? sender, EventArgs e)
    {
        var text = _log.GetAllLogs();
        if (string.IsNullOrEmpty(text))
        {
            MessageBox.Show("没有可复制的日志内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Clipboard.SetText(text);
        MessageBox.Show($"已复制 {text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length} 条日志", "复制成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnFormClosing(e);
    }
}