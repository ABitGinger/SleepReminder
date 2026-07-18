using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SleepReminder.Models;

namespace SleepReminder.Forms;

public class ExitHistoryForm : Form
{
    public ExitHistoryForm(List<ExitRecord> records)
    {
        Text = "SleepReminder - 退出历史记录";
        Size = new Size(500, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 10);

        var lbl = new Label
        {
            Text = $"所有退出记录 (共 {records.Count} 条):",
            Location = new Point(12, 12),
            Size = new Size(460, 22)
        };
        Controls.Add(lbl);

        var listBox = new ListBox
        {
            Location = new Point(12, 40),
            Size = new Size(460, 280),
            Font = new Font("Consolas", 10),
            IntegralHeight = false
        };

        foreach (var record in records)
            listBox.Items.Add(record.ToString());

        Controls.Add(listBox);

        var btnClose = new Button
        {
            Text = "关闭",
            Location = new Point(200, 330),
            Size = new Size(80, 30),
            Font = Font
        };
        btnClose.Click += (_, _) => Close();
        Controls.Add(btnClose);
    }
}