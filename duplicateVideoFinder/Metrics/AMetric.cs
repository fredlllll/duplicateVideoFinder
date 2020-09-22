using System;

namespace duplicateVideoFinder.Metrics
{
    public abstract class AMetric :IEquatable<AMetric>
    {
        public abstract override bool Equals(object obj);

        public abstract bool Equals(AMetric other);

        public abstract override int GetHashCode();
    }
}
