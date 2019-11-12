using duplicateVideoFinder.Metrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace duplicateVideoFinder
{
    /// <summary>
    /// a collection of files that are potential duplicates
    /// </summary>
    public class DupeFileCollection : List<FileInfo>
    {
        public readonly AMetric metric;

        public DupeFileCollection(AMetric metric)
        {
            this.metric = metric;
        }
    }
}
