using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder.Metrics
{
    public abstract class AMetric
    {
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }
}
