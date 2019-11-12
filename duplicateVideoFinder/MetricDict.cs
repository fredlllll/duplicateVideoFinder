using duplicateVideoFinder.Metrics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace duplicateVideoFinder
{
    public class MetricDict : ConcurrentDictionary<FileInfo, AMetric>
    {
        
    }
}
