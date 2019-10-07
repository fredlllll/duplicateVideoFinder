using duplicateVideoFinder.Metrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace duplicateVideoFinder.MetricGenerators
{
    public interface AMetricGenerator
    {
        AMetric Generate(FileInfo file);
    }
}
