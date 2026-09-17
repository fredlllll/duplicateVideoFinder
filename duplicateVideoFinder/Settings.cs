using System;
using System.IO;
using System.Text.Json.Nodes;

namespace duplicateVideoFinder
{
    public class Settings
    {
        public JsonNode Data { get; }

        public Settings(string file = "settings.json")
        {
            string path = FindSettingsFile(file);
            if (path == null)
            {
                throw new FileNotFoundException(
                    "settings.json not found. Expected it in the executable directory or the current working directory.",
                    file);
            }
            Data = JsonNode.Parse(File.ReadAllText(path));
        }

        private static string FindSettingsFile(string file)
        {
            string exeDir = AppContext.BaseDirectory;
            string exeDirPath = exeDir != null ? Path.Combine(exeDir, file) : null;
            if (exeDirPath != null && File.Exists(exeDirPath))
            {
                return exeDirPath;
            }
            string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), file);
            if (File.Exists(cwdPath))
            {
                return cwdPath;
            }
            return null;
        }
    }
}