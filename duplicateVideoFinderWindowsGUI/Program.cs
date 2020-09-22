using System;
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
            FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official).Wait(); //TODO: wrap this into a splash or so

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmStart());
        }
    }
}
