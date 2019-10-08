namespace duplicateVideoFinder.Metrics
{
    public class DurationMetric : AMetric
    {
        private readonly ulong duration;

        public DurationMetric(ulong duration)
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
