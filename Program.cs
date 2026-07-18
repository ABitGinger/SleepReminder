using System;
using System.Threading;
using System.Windows.Forms;

namespace SleepReminder;

internal static class Program
{
    private static readonly Mutex _mutex = new(true, "SleepReminder_SingleInstanceMutex");

    [STAThread]
    private static void Main()
    {
        if (!_mutex.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("SleepReminder 已在运行中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainApplication());
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}
