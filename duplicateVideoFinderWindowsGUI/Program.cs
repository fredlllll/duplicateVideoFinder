using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabe.FFmpeg.Downloader;

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
                try
                {
                    // downloads once into the app folder; subsequent runs skip the download
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
                }
                catch
                {
                    // FFmpeg unavailable: duration metrics just won't be computed
                }
                finally
                {
                    startup.BeginInvoke(new Action(() => startup.SetFFmpegReady(true)));
                }
            });
        }
    }
}