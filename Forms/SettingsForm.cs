#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using SleepReminder.Models;
using SleepReminder.Services;

namespace SleepReminder.Forms;

public class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly LogManager _log;
    private readonly Action<AppSettings> _onSave;

    private NumericUpDown nudShowHour;
    private NumericUpDown nudShowMinute;
    private NumericUpDown nudTargetHour;
    private NumericUpDown nudTargetMinute;
    private NumericUpDown nudHideHour;
    private NumericUpDown nudHideMinute;
    private NumericUpDown nudWidth;
    private NumericUpDown nudHeight;
    private NumericUpDown nudOpacity;
    private NumericUpDown nudEnlargeDuration;
    private NumericUpDown nudFontSize;
    private TextBox txtPosX;
    private TextBox txtPosY;
    private TextBox txtBgColor;
    private TextBox txtTextColor;
    private TextBox txtFullText;
    private TextBox txtFullTextColor;
    private CheckBox chkDebug;
    private CheckBox chkAutoStart;
    private Button btnPinStart;
    private Button btnSave;
    private Button btnCancel;

    public SettingsForm(AppSettings settings, LogManager log, Action<AppSettings> onSave)
    {
        _settings = settings.Clone();
        _log = log;
        _onSave = onSave;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "SleepReminder - 设置";
        Size = new Size(480, 620);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 10);

        var y = 12;
        var labelW = 130;
        var gap = 8;

        // Show Time
        Controls.Add(CreateLabel("显示时间:", 0, y, labelW, 26));
        nudShowHour = CreateNud(0, 23, labelW + gap, y, 50, 26);
        nudShowMinute = CreateNud(0, 59, labelW + gap + 60, y, 50, 26);
        Controls.Add(nudShowHour);
        Controls.Add(nudShowMinute);
        Controls.Add(CreateLabel("时 : 分", labelW + gap + 120, y + 2, 60, 22));
        y += 32;

        // Target Time
        Controls.Add(CreateLabel("目标时间:", 0, y, labelW, 26));
        nudTargetHour = CreateNud(0, 23, labelW + gap, y, 50, 26);
        nudTargetMinute = CreateNud(0, 59, labelW + gap + 60, y, 50, 26);
        Controls.Add(nudTargetHour);
        Controls.Add(nudTargetMinute);
        Controls.Add(CreateLabel("时 : 分", labelW + gap + 120, y + 2, 60, 22));
        y += 32;

        // Hide Time
        Controls.Add(CreateLabel("隐藏时间:", 0, y, labelW, 26));
        nudHideHour = CreateNud(0, 23, labelW + gap, y, 50, 26);
        nudHideMinute = CreateNud(0, 59, labelW + gap + 60, y, 50, 26);
        Controls.Add(nudHideHour);
        Controls.Add(nudHideMinute);
        Controls.Add(CreateLabel("时 : 分", labelW + gap + 120, y + 2, 60, 22));
        y += 32;

        y += 4;

        // Window Size
        Controls.Add(CreateLabel("窗口宽:", 0, y, labelW, 26));
        nudWidth = CreateNud(50, 2000, labelW + gap, y, 70, 26);
        Controls.Add(nudWidth);
        Controls.Add(CreateLabel("px", labelW + gap + 80, y + 2, 30, 22));
        y += 32;

        Controls.Add(CreateLabel("窗口高:", 0, y, labelW, 26));
        nudHeight = CreateNud(20, 2000, labelW + gap, y, 70, 26);
        Controls.Add(nudHeight);
        Controls.Add(CreateLabel("px", labelW + gap + 80, y + 2, 30, 22));
        y += 32;

        // Position
        Controls.Add(CreateLabel("位置X(-1自动):", 0, y, labelW, 26));
        txtPosX = CreateTextBox(labelW + gap, y, 70, 26);
        Controls.Add(txtPosX);
        y += 32;

        Controls.Add(CreateLabel("位置Y(-1自动):", 0, y, labelW, 26));
        txtPosY = CreateTextBox(labelW + gap, y, 70, 26);
        Controls.Add(txtPosY);
        y += 32;

        // Opacity
        Controls.Add(CreateLabel("透明度(%):", 0, y, labelW, 26));
        nudOpacity = CreateNud(5, 100, labelW + gap, y, 70, 26);
        Controls.Add(nudOpacity);
        y += 32;

        // Enlarge Duration
        Controls.Add(CreateLabel("放大时长(秒):", 0, y, labelW, 26));
        nudEnlargeDuration = CreateNud(10, 600, labelW + gap, y, 70, 26);
        Controls.Add(nudEnlargeDuration);
        y += 32;

        // Font Size
        Controls.Add(CreateLabel("字号:", 0, y, labelW, 26));
        nudFontSize = CreateNud(8, 200, labelW + gap, y, 70, 26);
        Controls.Add(nudFontSize);
        y += 32;

        // Colors
        Controls.Add(CreateLabel("文字颜色:", 0, y, labelW, 26));
        txtTextColor = CreateTextBox(labelW + gap, y, 100, 26);
        Controls.Add(txtTextColor);
        y += 32;

        Controls.Add(CreateLabel("背景颜色:", 0, y, labelW, 26));
        txtBgColor = CreateTextBox(labelW + gap, y, 100, 26);
        Controls.Add(txtBgColor);
        y += 32;

        Controls.Add(CreateLabel("全屏文本:", 0, y, labelW, 26));
        txtFullText = CreateTextBox(labelW + gap, y, 200, 26);
        Controls.Add(txtFullText);
        y += 32;

        Controls.Add(CreateLabel("全屏文字颜色:", 0, y, labelW, 26));
        txtFullTextColor = CreateTextBox(labelW + gap, y, 100, 26);
        Controls.Add(txtFullTextColor);
        y += 32;

        // Debug mode
        chkDebug = new CheckBox
        {
            Text = "调试模式",
            Location = new Point(labelW + gap, y),
            Size = new Size(150, 26),
            Font = Font
        };
        Controls.Add(chkDebug);
        y += 32;

        // Auto start
        chkAutoStart = new CheckBox
        {
            Text = "开机自启动",
            Location = new Point(labelW + gap, y),
            Size = new Size(150, 26),
            Font = Font
        };
        Controls.Add(chkAutoStart);
        y += 32;

        // Pin to Start Menu
        var pinText = SystemHelper.IsPinnedToStartMenu() ? "已固定到开始菜单" : "添加到开始菜单";
        btnPinStart = new Button
        {
            Text = pinText,
            Location = new Point(labelW + gap, y),
            Size = new Size(170, 30),
            Font = Font,
            Enabled = !SystemHelper.IsPinnedToStartMenu()
        };
        btnPinStart.Click += (_, _) =>
        {
            if (SystemHelper.PinToStartMenu())
            {
                btnPinStart.Text = "已固定到开始菜单";
                btnPinStart.Enabled = false;
                MessageBox.Show("已成功添加到开始菜单。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("添加到开始菜单失败，请手动创建快捷方式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        Controls.Add(btnPinStart);
        y += 40;

        // Buttons
        btnSave = new Button
        {
            Text = "保存",
            Location = new Point(120, y),
            Size = new Size(90, 32),
            Font = Font
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(220, y),
            Size = new Size(90, 32),
            Font = Font
        };
        btnCancel.Click += (_, _) => Close();
        Controls.Add(btnCancel);

        y += 50;
        ClientSize = new Size(460, y);
    }

    private Label CreateLabel(string text, int x, int y, int w, int h)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, h),
            TextAlign = ContentAlignment.MiddleRight,
            Font = Font
        };
    }

    private NumericUpDown CreateNud(int min, int max, int x, int y, int w, int h)
    {
        return new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = min,
            Location = new Point(x, y),
            Size = new Size(w, h),
            Font = Font
        };
    }

    private TextBox CreateTextBox(int x, int y, int w, int h)
    {
        return new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            Font = Font
        };
    }

    private void LoadSettings()
    {
        nudShowHour.Value = _settings.ShowHour;
        nudShowMinute.Value = _settings.ShowMinute;
        nudTargetHour.Value = _settings.TargetHour;
        nudTargetMinute.Value = _settings.TargetMinute;
        nudHideHour.Value = _settings.HideHour;
        nudHideMinute.Value = _settings.HideMinute;
        nudWidth.Value = _settings.WindowWidth;
        nudHeight.Value = _settings.WindowHeight;
        txtPosX.Text = _settings.WindowX.ToString();
        txtPosY.Text = _settings.WindowY.ToString();
        nudOpacity.Value = _settings.OpacityPercent;
        nudEnlargeDuration.Value = _settings.EnlargeDurationSeconds;
        nudFontSize.Value = (decimal)_settings.FontSize;
        txtTextColor.Text = _settings.CountdownTextColor;
        txtBgColor.Text = _settings.BackgroundColor;
        txtFullText.Text = _settings.FullscreenText;
        txtFullTextColor.Text = _settings.FullscreenTextColor;
        chkDebug.Checked = _settings.DebugMode;
        chkAutoStart.Checked = _settings.AutoStart;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        _settings.ShowHour = (int)nudShowHour.Value;
        _settings.ShowMinute = (int)nudShowMinute.Value;
        _settings.TargetHour = (int)nudTargetHour.Value;
        _settings.TargetMinute = (int)nudTargetMinute.Value;
        _settings.HideHour = (int)nudHideHour.Value;
        _settings.HideMinute = (int)nudHideMinute.Value;
        _settings.WindowWidth = (int)nudWidth.Value;
        _settings.WindowHeight = (int)nudHeight.Value;

        if (int.TryParse(txtPosX.Text, out int x)) _settings.WindowX = x;
        if (int.TryParse(txtPosY.Text, out int y)) _settings.WindowY = y;

        _settings.OpacityPercent = (int)nudOpacity.Value;
        _settings.EnlargeDurationSeconds = (int)nudEnlargeDuration.Value;
        _settings.FontSize = (float)nudFontSize.Value;
        _settings.CountdownTextColor = txtTextColor.Text.Trim();
        _settings.BackgroundColor = txtBgColor.Text.Trim();
        _settings.FullscreenText = txtFullText.Text.Trim();
        _settings.FullscreenTextColor = txtFullTextColor.Text.Trim();
        _settings.DebugMode = chkDebug.Checked;
        _settings.AutoStart = chkAutoStart.Checked;

        if (!_settings.Validate(out var error))
        {
            MessageBox.Show(error, "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _onSave(_settings);
        _log.Info("设置已保存到内存");
        Close();
    }
}