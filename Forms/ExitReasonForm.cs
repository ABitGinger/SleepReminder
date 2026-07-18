using System;
using System.Drawing;
using System.Windows.Forms;

namespace SleepReminder.Forms;

public class ExitReasonForm : Form
{
    private readonly TextBox _txtReason;
    private bool _submitted;

    public string Reason { get; private set; } = string.Empty;
    public bool Submitted => _submitted;

    public ExitReasonForm()
    {
        Text = "SleepReminder - 退出理由";
        Size = new Size(420, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 10);

        var lbl = new Label
        {
            Text = "请输入退出理由:",
            Location = new Point(12, 12),
            Size = new Size(380, 22)
        };
        Controls.Add(lbl);

        _txtReason = new TextBox
        {
            Location = new Point(12, 40),
            Size = new Size(380, 60),
            Multiline = true,
            Font = Font
        };
        Controls.Add(_txtReason);

        var btnOk = new Button
        {
            Text = "确定提交",
            Location = new Point(120, 118),
            Size = new Size(90, 30),
            Font = Font
        };
        btnOk.Click += BtnOk_Click;
        Controls.Add(btnOk);

        var btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(220, 118),
            Size = new Size(80, 30),
            Font = Font
        };
        btnCancel.Click += (_, _) => Close();
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        var reason = _txtReason.Text.Trim();
        if (string.IsNullOrEmpty(reason))
        {
            MessageBox.Show("退出理由不能为空，请输入有效内容。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Reason = reason;
        _submitted = true;
        Close();
    }
}