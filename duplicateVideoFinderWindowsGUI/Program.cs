using System;
using System.Windows.Forms;
using Xabe.FFmpeg;

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
            FFmpeg.GetLatestVersion().Wait(); //TODO: wrap this into a splash or so

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmStart());
        }
    }
}
