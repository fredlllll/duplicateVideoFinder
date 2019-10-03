using System;
using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("please provide a folder");
                return;
            }

            DirectoryInfo di = new DirectoryInfo(args[0]);

            var files = di.EnumerateFiles("*", SearchOption.AllDirectories);

            Dictionary<string, List<FileInfo>> comparisonResults = new Dictionary<string, List<FileInfo>>();

            var hashFileComparer = new HashFileComparer();

            foreach(FileInfo f in files)
            {
                string hash = hashFileComparer.HashToString(hashFileComparer.GetSmallHash(f));

                if(!comparisonResults.TryGetValue(hash,out List<FileInfo> fileList))
                {
                    fileList = new List<FileInfo>();
                    comparisonResults[hash] = fileList;
                }
                fileList.Add(f);
            }

            List<string> hashesToForget = new List<string>();
            foreach(var kv in comparisonResults)
            {
                if(kv.Value.Count == 1)
                {
                    hashesToForget.Add(kv.Key);
                }
            }
            foreach(var htf in hashesToForget)
            {
                comparisonResults.Remove(htf);
            }

            foreach(var kv in comparisonResults)
            {
                Console.WriteLine(kv.Key + ":");
                foreach(var f in kv.Value)
                {
                    Console.WriteLine(f.FullName);
                }
                Console.WriteLine();
            }
        }
    }
}
