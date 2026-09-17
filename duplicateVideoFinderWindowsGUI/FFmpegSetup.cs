using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using duplicateVideoFinder;

namespace duplicateVideoFinderWindowsGUI
{
    // Replaces Xabe.FFmpeg.Downloader. Downloads the Windows FFmpeg build once,
    // extracts ffprobe.exe into a per-user tools folder and points Ffprobe at it.
    internal static class FFmpegSetup
    {
        private const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        public static async Task<bool> EnsureFfprobeAsync()
        {
            string toolsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "duplicateVideoFinder",
                "ffmpeg");
            string probePath = Path.Combine(toolsDir, "ffprobe.exe");

            if (File.Exists(probePath))
            {
                Ffprobe.ExecutablePath = probePath;
                return true;
            }

            // ffprobe may already be resolvable (next to the exe or on PATH): skip the download
            if (Ffprobe.Resolve() != null)
            {
                return true;
            }

            Directory.CreateDirectory(toolsDir);
            string zipPath = Path.Combine(Path.GetTempPath(), "dvf-ffmpeg-" + Guid.NewGuid().ToString("N") + ".zip");
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromMinutes(10);
                    using var response = await http.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    using var file = File.Create(zipPath);
                    await response.Content.CopyToAsync(file);
                }

                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    var entry = archive.Entries.FirstOrDefault(
                        e => e.Name.Equals("ffprobe.exe", StringComparison.OrdinalIgnoreCase));
                    if (entry == null)
                    {
                        return false;
                    }
                    entry.ExtractToFile(probePath, true);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }
                }
                catch
                {
                    // temp cleanup is best-effort
                }
            }

            Ffprobe.ExecutablePath = probePath;
            return Ffprobe.Run("-version") != null;
        }
    }
}