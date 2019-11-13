using duplicateVideoFinder.Metrics;
using System.IO;

namespace duplicateVideoFinder.MetricGenerators
{
    public interface AMetricGenerator
    {
        string ID { get; }
        AMetric Generate(FileInfo file);
    }
}
