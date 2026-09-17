using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace duplicateVideoFinderWindowsGUI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var startup = new FrmStart();
            DoFFmpegSetupInBackground(startup);

            Application.Run(startup);
        }

        static void DoFFmpegSetupInBackground(FrmStart startup)
        {
            Task.Run(async () =>
            {
                bool ready = false;
                try
                {
                    // downloads once into a per-user tools folder; subsequent runs skip the download
                    ready = await FFmpegSetup.EnsureFfprobeAsync();
                }
                catch
                {
                    // FFmpeg unavailable: duration checkbox stays disabled
                }
                if (startup.IsHandleCreated)
                {
                    startup.BeginInvoke(new Action(() => startup.SetFFmpegReady(ready)));
                }
            });
        }
    }
}