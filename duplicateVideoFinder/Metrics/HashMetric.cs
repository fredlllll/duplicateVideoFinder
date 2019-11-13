namespace duplicateVideoFinder.Metrics
{
    public class HashMetric : AMetric
    {
        public byte[] hash;

        public HashMetric(byte[] hash = null)
        {
            this.hash = hash;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is HashMetric metric))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != metric.hash[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return hash[0]+hash[1]+hash[2];
        }
    }
}
