using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SleepReminder.Services;

public static class IconHelper
{
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon CreateSleepIcon(int size = 64)
    {
        using var bitmap = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            var rect = new Rectangle(0, 0, size, size);
            var radius = size / 5;
            using var path = GetRoundedRect(rect, radius);
            using var bgBrush = new SolidBrush(Color.Black);
            g.FillPath(bgBrush, path);

            using var textBrush = new SolidBrush(Color.White);
            var fontSize = size * 0.55f;
            using var font = new Font("SimHei", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("睡", font, textBrush, new RectangleF(0, -size * 0.03f, size, size), sf);
        }

        var hIcon = bitmap.GetHicon();
        try
        {
            using var tempIcon = Icon.FromHandle(hIcon);
            using var ms = new MemoryStream();
            tempIcon.Save(ms);
            ms.Position = 0;
            return new Icon(ms);
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var r2 = radius * 2;
        path.AddArc(rect.X, rect.Y, r2, r2, 180, 90);
        path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90);
        path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90);
        path.CloseFigure();
        return path;
    }
}
