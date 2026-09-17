using System.Globalization;
using System.IO;
using System.Text.Json;
using duplicateVideoFinder.Metrics;

namespace duplicateVideoFinder.MetricGenerators
{
    public class DurationMetricGenerator : AMetricGenerator
    {
        public string ID => "durationmetric";

        public AMetric Generate(FileInfo file)
        {
            try
            {
                string result = Ffprobe.Run("-v quiet -of json -show_format -show_streams \"" + file.FullName + "\"");
                if (string.IsNullOrEmpty(result))
                {
                    return null;
                }

                using var probe = JsonDocument.Parse(result);
                double? duration = null;

                if (probe.RootElement.TryGetProperty("format", out var format)
                    && TryGetDuration(format, out double formatDuration))
                {
                    duration = formatDuration;
                }

                if (!duration.HasValue
                    && probe.RootElement.TryGetProperty("streams", out var streams)
                    && streams.ValueKind == JsonValueKind.Array)
                {
                    foreach (var stream in streams.EnumerateArray())
                    {
                        bool isVideo = stream.TryGetProperty("codec_type", out var codecType)
                            && codecType.ValueKind == JsonValueKind.String
                            && "video".Equals(codecType.GetString());
                        if (isVideo && TryGetDuration(stream, out double streamDuration))
                        {
                            duration = streamDuration;
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
            }
            catch
            {
                // unparseable/ffprobe failure: treat as unknown duration
                return null;
            }
        }

        // ffprobe reports "duration" as a string (e.g. "5.000000") in most
        // builds, but occasionally as a number; accept both.
        private static bool TryGetDuration(JsonElement element, out double duration)
        {
            duration = 0;
            if (!element.TryGetProperty("duration", out var value))
            {
                return false;
            }
            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.TryGetDouble(out duration);
            }
            if (value.ValueKind == JsonValueKind.String)
            {
                return double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out duration);
            }
            return false;
        }
    }
}