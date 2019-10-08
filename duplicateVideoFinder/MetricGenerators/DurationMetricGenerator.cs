using System.IO;
using duplicateVideoFinder.Metrics;
using Microsoft.WindowsAPICodePack.Shell;

namespace duplicateVideoFinder.MetricGenerators
{
    public class DurationMetricGenerator : AMetricGenerator
    {
        public AMetric Generate(FileInfo file)
        {
            ShellFile shellFile = ShellFile.FromFilePath(file.FullName);
            ulong? duration = shellFile.Properties.System.Media.Duration.Value;
            if (duration.HasValue)
            {
                return new DurationMetric(duration.Value);
            }
            else
            {
                return null;
            }
        }
    }
}
