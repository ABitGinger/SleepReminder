using System;
using System.Threading;
using System.Windows.Forms;

namespace SleepReminder;

internal static class Program
{
    private static readonly Mutex _mutex = new(true, "SleepReminder_SingleInstanceMutex");

    [STAThread]
    private static void Main(string[] args)
    {
        if (!_mutex.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("SleepReminder 已在运行中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var firstRun = args?.Length > 0 && args[0] == "--first-run";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainApplication(firstRun));
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}
