using duplicateVideoFinder.Metrics;
using System.IO;

namespace duplicateVideoFinder.MetricGenerators
{
    public interface AMetricGenerator
    {
        AMetric Generate(FileInfo file);
    }
}
