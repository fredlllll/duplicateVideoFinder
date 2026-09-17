using System;
using System.Diagnostics;
using System.IO;

namespace duplicateVideoFinder
{
    /// <summary>
    /// Locates and runs ffprobe. The GUI sets <see cref="ExecutablePath"/> to the
    /// ffprobe it downloads; the console falls back to ffprobe on PATH or next to
    /// the executable. Replaces the old Xabe.FFmpeg Probe wrapper.
    /// </summary>
    public static class Ffprobe
    {
        private const string WindowsExeName = "ffprobe.exe";
        private const string UnixExeName = "ffprobe";

        /// <summary>Explicit path to ffprobe, usually set by the GUI after download.</summary>
        public static string ExecutablePath { get; set; }

        public static string Resolve()
        {
            if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
            {
                return ExecutablePath;
            }

            string exeName = OperatingSystem.IsWindows() ? WindowsExeName : UnixExeName;
            string local = Path.Combine(AppContext.BaseDirectory, exeName);
            if (File.Exists(local))
            {
                return local;
            }

            return FindOnPath(exeName);
        }

        private static string FindOnPath(string exeName)
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
            {
                return null;
            }
            foreach (string entry in pathVariable.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }
                try
                {
                    string candidate = Path.Combine(entry.Trim(), exeName);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch
                {
                    // malformed PATH entry: skip it
                }
            }
            return null;
        }

        /// <summary>Runs ffprobe with the given arguments and returns stdout, or null on failure.</summary>
        public static string Run(string arguments)
        {
            string exe = Resolve();
            if (exe == null)
            {
                return null;
            }
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return null;
                }
                var outputTask = process.StandardOutput.ReadToEndAsync();
                _ = process.StandardError.ReadToEndAsync(); // drain stderr so the process can't block on a full pipe
                if (!process.WaitForExit(30000))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                        // already gone
                    }
                    return null;
                }
                process.WaitForExit(); // let the redirected streams finish flushing
                return outputTask.GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }
    }
}