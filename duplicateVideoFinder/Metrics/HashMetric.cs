using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace duplicateVideoFinder.Metrics
{
    public class HashMetric : AMetric
    {
        private readonly byte[] hash;

        public HashMetric(byte[] hash)
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
