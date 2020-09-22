using duplicateVideoFinder.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder
{
    public static class PotentialDuplicateFinder
    {
        public static DupeCollection FindDupes(MetricDict metrics)
        {
            Dictionary<AMetric, DupeFileCollection> filesByMetrics = new Dictionary<AMetric, DupeFileCollection>();
            foreach (var kv in metrics)
            {
                if (kv.Value == null)
                {
                    continue;
                }
                DupeFileCollection dfc;
                if (!filesByMetrics.TryGetValue(kv.Value, out dfc))
                {
                    dfc = new DupeFileCollection(kv.Value);
                    filesByMetrics[kv.Value] = dfc;
                }
                dfc.Add(kv.Key);
            }

            DupeCollection dc = new DupeCollection();

            foreach (var kv in filesByMetrics)
            {
                if (kv.Value.Count > 1)
                {
                    dc.Add(kv.Value);
                }
            }
            return dc;
        }
    }
}
