using System.IO;
using System.Security.Cryptography;
using duplicateVideoFinder.Metrics;

namespace duplicateVideoFinder.MetricGenerators
{
    public class HashMetricGenerator : AMetricGenerator
    {
        private readonly int hashedBytesCount;

        public HashMetricGenerator(int hashedBytesCount = 256)
        {
            this.hashedBytesCount = hashedBytesCount;
        }

        public AMetric Generate(FileInfo file)
        {
            byte[] hash = null;
            using (FileStream fs = file.OpenRead())
            {
                if (fs.Length < 4 * hashedBytesCount)
                {
                    //if file is small enough just compute hash over the whole file, that way we save us the pain of checking for out of bounds etc
                    using (MD5 md5 = MD5.Create())
                    {
                        hash = md5.ComputeHash(fs);
                    }
                }
                else
                {
                    using (IncrementalHash incMd5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
                    {
                        byte[] block = new byte[hashedBytesCount];
                        //start
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(block, 0, block.Length);
                        incMd5.AppendData(block);

                        //middle
                        fs.Seek(fs.Length / 2 - hashedBytesCount / 2, SeekOrigin.Begin);
                        fs.Read(block, 0, block.Length);
                        incMd5.AppendData(block);

                        //end
                        fs.Seek(-hashedBytesCount, SeekOrigin.End);
                        fs.Read(block, 0, block.Length);
                        incMd5.AppendData(block);

                        hash = incMd5.GetHashAndReset();
                    }
                }
            }
            return new HashMetric(hash);
        }
    }
}
