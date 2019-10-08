using Newtonsoft.Json.Linq;
using System.IO;

namespace duplicateVideoFinder
{
    public class Settings
    {
        public static readonly Settings appSettings = new Settings();

        public JObject Data { get; }

        public Settings(string file = "settings.json")
        {
            if (File.Exists(file))
            {
                Data = JObject.Parse(File.ReadAllText(file));
            }
            else
            {
                Data = new JObject();
            }
        }
    }
}
