namespace duplicateVideoFinder.Metrics
{
    public class DurationMetric : AMetric
    {
        public ulong duration;

        public DurationMetric(ulong duration = 0)
        {
            this.duration = duration;
        }
        public override bool Equals(object obj)
        {
            if(!( obj is DurationMetric other))
            {
                return false;
            }
            return duration == other.duration;
        }

        public override int GetHashCode()
        {
            return (int)duration;
        }
    }
}
