using System.IO;
using duplicateVideoFinder.Metrics;
using Newtonsoft.Json.Linq;
using Xabe.FFmpeg;

namespace duplicateVideoFinder.MetricGenerators
{
    public class DurationMetricGenerator : AMetricGenerator
    {
        public string ID => "durationmetric";

        public AMetric Generate(FileInfo file)
        {
            var t = Probe.New().Start("-v quiet -of json -show_format -show_streams \"" + file.FullName+"\"");
            t.Wait();
            string result = t.Result;
            JObject probe = JObject.Parse(result);
            JArray streams = probe["streams"] as JArray;
            JObject format = probe["format"] as JObject;
            ulong? duration = null;
            if(format != null && format["duration_ts"] != null)
            {
                duration = format["duration_ts"].Value<ulong>();
            }
            if (!duration.HasValue && streams != null)
            {
                foreach(JObject stream in streams)
                {
                    string codecType = stream["codec_type"]?.Value<string>();
                    if ("video".Equals(codecType))
                    {
                        duration = stream["duration_ts"]?.Value<ulong>();
                    }
                    if (duration.HasValue)
                    {
                        break;
                    }
                }
            }
            if (duration.HasValue)
            {
                return new DurationMetric(duration.Value);
            }
            return null;
            /*ShellFile shellFile = ShellFile.FromFilePath(file.FullName);
            ulong? duration = shellFile.Properties.System.Media.Duration.Value;
            if (duration.HasValue)
            {
                return new DurationMetric(duration.Value);
            }
            else
            {
                return null;
            }*/
        }
    }
}
