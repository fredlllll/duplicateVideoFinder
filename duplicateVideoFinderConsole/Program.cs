using duplicateVideoFinder;
using duplicateVideoFinder.MetricGenerators;
using System;
using System.IO;

namespace duplicateVideoFinderConsole
{
    class Program
    {
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
        private static void Finder_OnProgress(duplicateVideoFinder.Progresses.IProgress progress)
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
            IDuplicateFinder finder = new DuplicateFinder(new AMetricGenerator[] { hashMetricGen }, di);
            finder.OnProgress += Finder_OnProgress;

            Console.WriteLine("Generating metrics");
            var result = finder.FindDuplicates();

            Console.WriteLine("Finding Dupes");
            var dupes = result.dupesByGenerator[hashMetricGen.ID];

            Console.WriteLine("Autosorting Dupes");
            int autosorted = 0;
            int errors = 0;
            foreach (var dupeFileCollection in dupes)
            {
                var toKeep = DuplicateKeeper.GetFileToKeep(dupeFileCollection);
                if (toKeep == null)
                {
                    continue;
                }
                foreach (var file in dupeFileCollection)
                {
                    if (file.FullName != toKeep.FullName
                        && file.FullName.IndexOf("UNSORTED", StringComparison.OrdinalIgnoreCase) >= 0
                        && file.Exists)
                    {
                        try
                        {
                            file.Delete(); //delete file in UNSORTED
                            autosorted++;
                        }
                        catch (Exception)
                        {
                            errors++; //keep scanning; a locked file must not abort the sort
                        }
                    }
                }
            }

            Console.WriteLine("Autosorted " + autosorted + " files" + (errors > 0 ? " (" + errors + " failed)" : ""));
            if (isInteractive)
            {
                Console.WriteLine("Press any key to end");
                Console.ReadKey();
            }
        }
    }
}