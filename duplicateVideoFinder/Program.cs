using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace duplicateVideoFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("please provide a folder");
                return;
            }

            Console.WriteLine("fuck me");
            Console.Clear();

            DirectoryInfo di = new DirectoryInfo(args[0]);

            var finder = new DuplicateFinder(null); //TODO: depends on input

            finder.OnProgress += Finder_OnProgress;

            var dupes = finder.FindDuplicates(di);

            JArray result = new JArray();

            foreach (var gen in dupes.dupeListsByGenerator)
            {
                foreach (var dupe in gen)
                {
                    JArray fileList = new JArray();
                    foreach (var fi in dupe)
                    {
                        fileList.Add(fi.FullName);
                    }
                    result.Add(fileList);
                }
            }

            Console.WriteLine(result.ToString());
        }

        private static void Finder_OnProgress(Progresses.IProgress progress)
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.Write("".PadLeft(Console.BufferWidth, ' '));
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.Write("Progress: " + progress.ToString());
        }
    }
}
