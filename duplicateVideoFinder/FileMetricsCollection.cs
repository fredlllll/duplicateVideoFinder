using duplicateVideoFinder.Metrics;
using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    public class FileMetricsCollection : List<AMetric>
    {
        public FileInfo File { get; private set; }

        public FileMetricsCollection(FileInfo file)
        {
            File = file;
        }
    }
}
