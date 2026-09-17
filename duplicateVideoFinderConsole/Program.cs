using duplicateVideoFinder;
using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinderConsole
{
    class Program
    {
        private class ConsoleProcessReceiver : IProgressReceiver
        {
            public void Update(IProgress progress)
            {
                Finder_OnProgress(progress);
            }
        }

        private static readonly bool isInteractive;

        static Program()
        {
            try
            {
                isInteractive = !Console.IsOutputRedirected && !Console.IsInputRedirected;
            }
            catch
            {
                isInteractive = false;
            }
        }

        private static object consoleLock = new object();
        private static void Finder_OnProgress(IProgress progress)
        {
            if (!isInteractive)
            {
                return;
            }
            lock (consoleLock)
            {
                int top = Console.CursorTop;
                int left = Console.CursorLeft;

                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("".PadLeft(Console.BufferWidth, ' '));
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("Progress: " + progress.ToString());

                Console.CursorTop = top;
                Console.CursorLeft = left;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("please provide a folder");
                return;
            }

            if (isInteractive)
            {
                Console.Clear();
                Console.WriteLine();
            }

            DirectoryInfo di = new DirectoryInfo(args[0]);
            if (!di.Exists)
            {
                Console.WriteLine("folder does not exist: " + args[0]);
                return;
            }

            var hashMetricGen = new HashMetricGenerator();
            var folderMetricGen = new FolderMetricGenerator(hashMetricGen, di, AppSettings.Instance.extensionsToProcess, SearchOption.AllDirectories);

            Console.WriteLine("Generating metrics");
            var metrics = folderMetricGen.GenerateMetrics(new ConsoleProcessReceiver());

            Console.WriteLine("Finding Dupes");
            var dupes = PotentialDuplicateFinder.FindDupes(metrics);

            Console.WriteLine("Autosorting Dupes");
            int autosorted = 0;
            foreach (var dupeFileCollection in dupes)
            {
                var toKeep = DuplicateKeeper.GetFileToKeep(dupeFileCollection);
                if (toKeep == null)
                {
                    continue;
                }
                foreach (var file in dupeFileCollection)
                {
                    if (file.FullName != toKeep.FullName && file.FullName.Contains("UNSORTED") && file.Exists)
                    {
                        autosorted++;
                        file.Delete(); //delete file in UNSORTED
                    }
                }
            }

            Console.WriteLine("Autosorted " + autosorted + " files");
            if (isInteractive)
            {
                Console.WriteLine("Press any key to end");
                Console.ReadKey();
            }
        }
    }
}