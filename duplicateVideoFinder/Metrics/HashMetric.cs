using System;
using System.Collections.Generic;

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
            return Equals(obj as AMetric);
        }

        public override bool Equals(AMetric other)
        {
            if (!(other is HashMetric metric))
            {
                return false;
            }
            if (ReferenceEquals(this, metric))
            {
                return true;
            }
            if (hash == null || metric.hash == null || hash.Length != metric.hash.Length)
            {
                return false;
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
            if (hash == null || hash.Length == 0)
            {
                return 0;
            }
            unchecked
            {
                int result = 17;
                for (int i = 0; i < hash.Length; i++)
                {
                    result = result * 31 + hash[i];
                }
                return result;
            }
        }
    }
}