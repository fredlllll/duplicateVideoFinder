using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace duplicateVideoFinder
{
    public class HashFileComparer
    {
        int hashedBytesCount = 512;

        public byte[] GetSmallHash(FileInfo f)
        {
            byte[] hash = null;
            using (FileStream fs = f.OpenRead())
            {
                if (fs.Length < 4 * hashedBytesCount)
                {
                    //if file is small enough just compute hash over the whole file, that way we save us the pain of checking for out of bounds etc
                    MD5 md5 = MD5.Create();
                    hash = md5.ComputeHash(fs);
                }
                else
                {
                    byte[] block = new byte[hashedBytesCount];
                    using (IncrementalHash md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5))

                    {
                        //start
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(block, 0, block.Length);
                        md5.AppendData(block);

                        //middle
                        fs.Seek(fs.Length / 2 - hashedBytesCount / 2, SeekOrigin.Begin);
                        fs.Read(block, 0, block.Length);
                        md5.AppendData(block);

                        //end
                        fs.Seek(-hashedBytesCount, SeekOrigin.End);
                        fs.Read(block, 0, block.Length);
                        md5.AppendData(block);

                        hash = md5.GetHashAndReset();
                    }
                }
            }
            return hash;
        }

        public bool AreTheSame(byte[] a, byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    //if hashes are not same return false
                    return false;
                }
            }
            return true;
        }


        public string HashToString(byte[] h)
        {
            return BitConverter.ToString(h).Replace("-", string.Empty);
        }
    }
}
