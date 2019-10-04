using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace duplicateVideoFinder
{
    public class HashDuplicateFinder : IDuplicateFinder
    {
        int hashedBytesCount;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public HashDuplicateFinder(int hashedBytesCount = 512)
        {
            this.hashedBytesCount = hashedBytesCount;
        }

        protected byte[] GetSmallHash(FileInfo f)
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

        protected bool AreTheSame(byte[] a, byte[] b)
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

        protected string HashToString(byte[] h)
        {
            return BitConverter.ToString(h).Replace("-", string.Empty);
        }

        public DuplicateCollection FindDuplicates(DirectoryInfo directory)
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));
            var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);
            int fileCount = 1;
            var fileCountTask = new Task(() => {
                var getFiles = directory.GetFiles("*", SearchOption.AllDirectories);
                fileCount = getFiles.Length;
            });
            fileCountTask.Start();

            Dictionary<string, List<FileInfo>> comparisonResults = new Dictionary<string, List<FileInfo>>();

            var hashFileComparer = new HashDuplicateFinder();

            int i = 0;
            foreach (FileInfo f in files)
            {
                string hash = hashFileComparer.HashToString(hashFileComparer.GetSmallHash(f));

                if (!comparisonResults.TryGetValue(hash, out List<FileInfo> fileList))
                {
                    fileList = new List<FileInfo>();
                    comparisonResults[hash] = fileList;
                }
                fileList.Add(f);

                OnProgress?.Invoke(new FractionalProgress(i,fileCount,f.FullName));
                i++;
            }

            List<string> hashesToForget = new List<string>();
            foreach (var kv in comparisonResults)
            {
                if (kv.Value.Count == 1)
                {
                    hashesToForget.Add(kv.Key);
                }
            }
            foreach (var htf in hashesToForget)
            {
                comparisonResults.Remove(htf);
            }

            DuplicateCollection dc = new DuplicateCollection();
            foreach (var kv in comparisonResults)
            {
                dc.Add(kv.Value);
            }
            OnProgress?.Invoke(new BasicProgress(1, "Done"));
            return dc;
        }
    }
}
