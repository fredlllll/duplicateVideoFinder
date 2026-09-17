using System;

namespace duplicateVideoFinder.Metrics
{
    public class DurationMetric : AMetric
    {
        public double duration;

        public DurationMetric(double duration = 0)
        {
            this.duration = duration;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AMetric);
        }

        public override bool Equals(AMetric other)
        {
            if (!(other is DurationMetric metric))
            {
                return false;
            }
            return duration == metric.duration;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(duration);
        }
    }
}